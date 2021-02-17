using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using Database.Data;

namespace UtiliSite
{
    public class PaymentsController : Controller
    {
        private static IStripeClient _stripeClient;
        public static void Initialise()
        {
            StripeConfiguration.ApiKey = Main.Config.StripePrivateKey;
            _stripeClient = new StripeClient(Main.Config.StripePrivateKey);
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest req)
        {
            await CreateCustomerIfRequiredAsync();

            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated)
            {
                return Forbid();
            }

            SessionCreateOptions options = new SessionCreateOptions
            {
                SuccessUrl = $"https://{HttpContext.Request.Host}/premium/success",
                CancelUrl = $"https://{HttpContext.Request.Host}/premium",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = req.PriceId,
                        // For metered billing, do not pass quantity
                        Quantity = 1,
                    },
                },
                Customer = auth.UserRow.CustomerId,
                AllowPromotionCodes = true
            };
            SessionService service = new SessionService(_stripeClient);
            try
            {
                Session session = await service.CreateAsync(options);
                return Ok(new CreateCheckoutSessionResponse
                {
                    SessionId = session.Id,
                });
            }
            catch (StripeException e)
            {
                Console.WriteLine(e.StripeError.Message);
                return BadRequest(new ErrorResponse
                {
                    ErrorMessage = new ErrorMessage
                    {
                        Message = e.StripeError.Message,
                    }
                });
            }
        }

        [HttpPost("customer-portal")]
        public async Task<IActionResult> CustomerPortal()
        {
            await CreateCustomerIfRequiredAsync();

            AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
            if (!auth.Authenticated)
            {
                return Forbid();
            }

            Stripe.BillingPortal.SessionCreateOptions options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = auth.UserRow.CustomerId,
                ReturnUrl = $"https://{HttpContext.Request.Host}/premium",
            };

            Stripe.BillingPortal.SessionService service = new Stripe.BillingPortal.SessionService(_stripeClient);
            Stripe.BillingPortal.Session session = await service.CreateAsync(options);
            return Ok(session);
        }

        private bool _creatingCustomer;
        public async Task CreateCustomerIfRequiredAsync()
        {
            if (_creatingCustomer)
            {
                while (_creatingCustomer) await Task.Delay(500);
                _creatingCustomer = true;
                await Task.Delay(500);
            }
            
            _creatingCustomer = true;

            try
            {
                AuthDetails auth = await Auth.GetAuthDetailsAsync(this);
                if (!auth.Authenticated)
                {
                    _creatingCustomer = false;
                    return;
                }

                if (!string.IsNullOrEmpty(auth.UserRow.CustomerId))
                {
                    _creatingCustomer = false;
                    return;
                }

                CustomerCreateOptions options = new CustomerCreateOptions {Description = $"User Id: {auth.User.Id}", Email = auth.User.Email};

                CustomerService service = new CustomerService(_stripeClient);
                Customer customer = await service.CreateAsync(options);

                auth.UserRow.CustomerId = customer.Id;
                await Users.SaveRowAsync(auth.UserRow);
                await Task.Delay(500);
            }
            catch
            {
                _creatingCustomer = false;
                throw;
            }
        }

        public static async Task<(string, bool)> GetCustomerCurrencyAsync(string customerId, HttpRequest request)
        {
            if (!string.IsNullOrEmpty(customerId))
            {
                CustomerService service = new CustomerService(_stripeClient);
                Customer customer = await service.GetAsync(customerId);
                if(!string.IsNullOrEmpty(customer.Currency)) return (customer.Currency, true);
            }

            return (await GetCustomerCurrencyByIpAsync(request), false);
        }

        private static async Task<string> GetCustomerCurrencyByIpAsync(HttpRequest request)
        {
            string[] gbp = {
                "GB", "IM", "JE", "GG"
            };
            string[] eur = {
                "AX", "EU", "AD", "AT", "BE", "CY", "EE", "FI", "FR", "TF", "DE", "GR", "GP", "IE", "IT", "LV", "LT",
                "LU", "MT", "GF", "MQ", "YT", "MC", "ME", "NL", "PT", "RE", "BL", "MF", "PM", "SM", "SK", "SI", "ES",
                "VA"
            };
            string[] usd = {
                "US", "AS", "IO", "VG", "BQ", "EC", "SV", "GU", "HT", "MH", "FM", "MP", "PW", "PA", "PR", "TL", "TC", 
                "VI", "UM"
            };

            HttpClient client = new HttpClient();

            string response = await client.GetStringAsync($"https://ipinfo.io/{request.HttpContext.Connection.RemoteIpAddress}/json");
            string country = JsonConvert.DeserializeObject<IpResponse>(response).Country;

            if (gbp.Contains(country)) return "gbp";
            if (eur.Contains(country)) return "eur";
            if (usd.Contains(country)) return "usd";
            return "usd";
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> Webhook()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                Main.Config.StripeWebhookSecret
            );

            Console.WriteLine($"{DateTime.Now} Stripe Webhook: {stripeEvent.Type}");

            // Webhooks are retried once an hour for up to 3 days or until a 200 status code is returned.

            switch (stripeEvent.Type) {
                case "customer.subscription.updated":
                    
                    string jsonSubscription = stripeEvent.Data.Object.ToString();
                    jsonSubscription = jsonSubscription.Substring(jsonSubscription.IndexOf('{'));
                    Subscription subscription = Subscription.FromJson(jsonSubscription);

                    ProductService productService = new ProductService(_stripeClient);
                    Product product = await productService.GetAsync(subscription.Items.Data[0].Plan.ProductId);
                    if (!int.TryParse(product.Metadata["slots"], out int slots)) slots = 0;

                    SubscriptionsRow row = await Subscriptions.GetRowAsync(subscription.Id);
                    row.Slots = slots;
                    row.EndsAt = subscription.CurrentPeriodEnd.AddHours(2);
                    row.Status = subscription.Status switch
                    {
                        "active" => SubscriptionStatus.Active,
                        "past_due" => SubscriptionStatus.PastDue,
                        "unpaid" => SubscriptionStatus.Unpaid,
                        "canceled" => SubscriptionStatus.Canceled,
                        "incomplete" => SubscriptionStatus.Incomplete,
                        "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
                        "trialing" => SubscriptionStatus.Trialing,
                        _ => throw new ArgumentException($"Unknown value for subscription {subscription.Id} status: {subscription.Status}")
                    };
                    await Subscriptions.SaveRowAsync(row);

                    break;

                default:
                    throw new ArgumentException($"Unknown Stripe event type {stripeEvent.Type}");
            }

            return Ok();
        }
    }

    #region Json

    public class CreateCheckoutSessionRequest
    {
        [JsonProperty("priceId")]
        public string PriceId { get; set; }
    }

    public class CreateCheckoutSessionResponse
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }

    public class ErrorMessage
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class ErrorResponse
    {
        [JsonProperty("error")]
        public ErrorMessage ErrorMessage { get; set; }
    }

    public class PublicKeyResponse
    {
        public string PublicKey { get; set; }
    }

    public class SetupResponse
    {
        [JsonProperty("publishableKey")]
        public string PublishableKey { get; set; }

        [JsonProperty("proPrice")]
        public string ProPrice { get; set; }

        [JsonProperty("basicPrice")]
        public string BasicPrice { get; set; }
    }

    public class CustomerPortalRequest
    {
        [JsonProperty("sessionId")]
        public string SessionId { get; set; }
    }

    public class InvoiceObject
    {
        [JsonProperty("customer")]
        public string CustomerId { get; set; }

        [JsonProperty("subscription")]
        public string SubscriptionId { get; set; }
    }

    public class IpResponse
    {
        [JsonProperty("country")]
        public string Country { get; set; }
    }

    #endregion
}
