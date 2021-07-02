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
using UtiliBackend;
using UtiliBackend.Controllers;

namespace UtiliSite
{
    public class StripeController : Controller
    {
        static IStripeClient _stripeClient;
        static List<ulong> _creatingCustomersFor = new List<ulong>();

        public static void Initialise()
        {
            StripeConfiguration.ApiKey = Main.Config.StripePrivateKey;
            _stripeClient = new StripeClient(Main.Config.StripePrivateKey);
        }

        [HttpPost("stripe/create-checkout-session")]
        public async Task<ActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest req)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            await CreateCustomerIfRequiredAsync(HttpContext);

            SessionCreateOptions options = new()
            {
                SuccessUrl = $"{Main.Config.Frontend}/premium/thankyou",
                CancelUrl = $"{Main.Config.Frontend}/premium",
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = req.PriceId,
                        Quantity = 1
                    }
                },
                Customer = auth.UserRow.CustomerId,
                AllowPromotionCodes = true
            };
            SessionService service = new(_stripeClient);
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

        [HttpGet("stripe/customer-portal")]
        public async Task<ActionResult> CustomerPortal()
        {
            await CreateCustomerIfRequiredAsync(HttpContext);
            await Task.Delay(500);

            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            if (string.IsNullOrEmpty(auth.UserRow.CustomerId)) throw new Exception("Customer id was null or empty");

            Stripe.BillingPortal.SessionCreateOptions options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = auth.UserRow.CustomerId,
                ReturnUrl = $"{Main.Config.Frontend}/premium",
            };

            Stripe.BillingPortal.SessionService service = new(_stripeClient);
            Stripe.BillingPortal.Session session = await service.CreateAsync(options);
            return Ok(session);
        }

        static async Task CreateCustomerIfRequiredAsync(HttpContext httpContext)
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(httpContext);
            if (!auth.Authorised) return;
            if (!string.IsNullOrEmpty(auth.UserRow.CustomerId)) return;

            bool wait = false;
            lock (_creatingCustomersFor)
            {
                if (_creatingCustomersFor.Contains(auth.User.Id)) wait = true;
                else _creatingCustomersFor.Add(auth.User.Id);
            }

            if (wait)
            {
                while (true)
                {
                    await Task.Delay(1000);
                    auth = await Authentication.GetAuthDetailsAsync(httpContext);
                    if (!string.IsNullOrEmpty(auth.UserRow.CustomerId)) return;
                }
            }

            CustomerCreateOptions options = new CustomerCreateOptions {Description = $"User Id: {auth.User.Id}", Email = auth.User.Email};

            CustomerService service = new(_stripeClient);
            Customer customer = await service.CreateAsync(options);

            auth.UserRow.CustomerId = customer.Id;
            await Users.SaveRowAsync(auth.UserRow);
        }

        [HttpGet("stripe/currency")]
        public async Task<ActionResult> Currency()
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            if (!string.IsNullOrEmpty(auth.UserRow.CustomerId))
            {
                CustomerService service = new(_stripeClient);
                Customer customer = await service.GetAsync(auth.UserRow.CustomerId);
                if (!string.IsNullOrEmpty(customer.Currency))
                    return new JsonResult(new CurrencyBody(customer.Currency, true));
            }

            string currencyCode = await GetCustomerCurrencyByIpAsync(HttpContext.Request);
            return new JsonResult(new CurrencyBody(currencyCode, false));
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

            HttpClient client = new();

            string response = await client.GetStringAsync($"https://ipinfo.io/{request.HttpContext.Connection.RemoteIpAddress}/json");
            string country = JsonConvert.DeserializeObject<IpResponse>(response).Country;

            if (gbp.Contains(country)) return "gbp";
            if (eur.Contains(country)) return "eur";
            if (usd.Contains(country)) return "usd";
            return "gbp";
        }

        [HttpGet("stripe/subscriptions")]
        public async Task<ActionResult> Subscriptions()
        {
            AuthDetails auth = await Authentication.GetAuthDetailsAsync(HttpContext);
            if (!auth.Authorised) return auth.Action;

            List<SubscriptionsRow> subscriptions = await Database.Data.Subscriptions.GetRowsAsync(userId: auth.User.Id);
            return new JsonResult(subscriptions.Select(x => new SubscriptionBody(x)));
        }

        [HttpPost("stripe/webhook")]
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
                case "customer.subscription.created":
                case "customer.subscription.updated":
                case "customer.subscription.deleted":
                    
                    string jsonSubscription = stripeEvent.Data.Object.ToString();
                    jsonSubscription = jsonSubscription.Substring(jsonSubscription.IndexOf('{'));
                    Subscription subscription = Subscription.FromJson(jsonSubscription);

                    if (subscription.Id == "sub_JIYbFIjPeNjEuy" && DateTime.UtcNow < new DateTime(2021, 04, 16))
                    {
                        return Ok();
                    }

                    ProductService productService = new ProductService(_stripeClient);
                    Product product = await productService.GetAsync(subscription.Items.Data[0].Plan.ProductId);
                    if (!int.TryParse(product.Metadata["slots"], out int slots)) slots = 0;

                    UserRow user = await Users.GetRowAsync(subscription.CustomerId);

                    SubscriptionsRow row = await Database.Data.Subscriptions.GetRowAsync(subscription.Id);
                    row.UserId = user.UserId;
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
                    await Database.Data.Subscriptions.SaveRowAsync(row);

                    break;

                default:
                    throw new ArgumentException($"Unknown Stripe event type {stripeEvent.Type}");
            }

            return Ok();
        }
    }

    public class CurrencyBody
    {
        public string CurrencyCode { get; set; }
        public bool Locked { get; set; }

        public CurrencyBody(string currencyCode, bool locked)
        {
            CurrencyCode = currencyCode.ToUpper();
            Locked = locked;
        }
    }

    public class SubscriptionBody
    {
        public int Slots { get; set; }
        public SubscriptionStatus Status { get; set; }

        public SubscriptionBody(SubscriptionsRow row)
        {
            Slots = row.Slots;
            Status = row.Status;
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
