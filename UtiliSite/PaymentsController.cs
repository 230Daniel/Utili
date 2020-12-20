using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;
using static UtiliSite.Main;
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
            StripeConfiguration.ApiKey = _config.StripePrivateKey;
            _stripeClient = new StripeClient(_config.StripePrivateKey);
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest req)
        {
            await CreateCustomerIfRequiredAsync(HttpContext);
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if (!auth.Authenticated)
            {
                return Forbid();
            }

            SessionCreateOptions options = new SessionCreateOptions
            {
                // See https://stripe.com/docs/api/checkout/sessions/create
                // for additional parameters to pass.
                // {CHECKOUT_SESSION_ID} is a string literal; do not change it!
                // the actual Session ID is returned in the query parameter when your customer
                // is redirected to the success page.
                SuccessUrl = $"https://{HttpContext.Request.Host}/premium/success?session_id={{CHECKOUT_SESSION_ID}}",
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
                Customer = auth.UserRow.CustomerId
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

        [HttpGet("checkout-session")]
        public async Task<IActionResult> CheckoutSession(string sessionId)
        {
            SessionService service = new SessionService(_stripeClient);
            Session session = await service.GetAsync(sessionId);
            return Ok(session);
        }

        [HttpPost("customer-portal")]
        public async Task<IActionResult> CustomerPortal([FromBody] CustomerPortalRequest req)
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(HttpContext, HttpContext.Request.Path);
            if (!auth.Authenticated)
            {
                return Forbid();
            }

            Stripe.BillingPortal.SessionCreateOptions options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = auth.UserRow.CustomerId,
                ReturnUrl = $"https://{HttpContext.Request.Host}/premium"
            };

            Stripe.BillingPortal.SessionService service = new Stripe.BillingPortal.SessionService(_stripeClient);
            Stripe.BillingPortal.Session session = await service.CreateAsync(options);
            return Ok(session);
        }

        public async Task CreateCustomerIfRequiredAsync(HttpContext httpContext)
        {
            AuthDetails auth = await Auth.GetAuthDetailsAsync(httpContext, null);
            if (!auth.Authenticated) return;
            if (!string.IsNullOrEmpty(auth.UserRow.CustomerId)) return;

            CustomerCreateOptions options = new CustomerCreateOptions
            {
                Description = $"User Id: {auth.User.Id}",
                Email = auth.User.Email
            };

            CustomerService service = new CustomerService(_stripeClient);
            Customer customer = await service.CreateAsync(options);

            auth.UserRow.CustomerId = customer.Id;
            Users.SaveRow(auth.UserRow);
        }

        [HttpPost("stripe-webhook")]
        public async Task<IActionResult> Webhook()
        {
            string json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _config.StripeWebhookSecret
                );
                Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something failed {e}");
                return BadRequest();
            }

            switch (stripeEvent.Type) {
                case "invoice.paid":
                    // Triggers on first payment and following payments

                    string jsonInvoice = stripeEvent.Data.Object.ToString();
                    jsonInvoice = jsonInvoice.Substring(jsonInvoice.IndexOf('{'));
                    Invoice invoice = Invoice.FromJson(jsonInvoice);
                    Console.WriteLine(jsonInvoice);

                    SubscriptionsRow row = Subscriptions.GetRow(invoice.SubscriptionId);
                    if (row.New)
                    {
                        ProductService service = new ProductService();
                        Product product = await service.GetAsync(invoice.Lines.Data.First().Plan.ProductId);

                        UserRow user = Users.GetRow(invoice.CustomerId);
                        row.Slots = int.Parse(product.Metadata["slots"]);
                        row.UserId = user.UserId;
                        row.EndsAt = DateTime.UtcNow.AddDays(30).AddHours(6);

                        Subscriptions.SaveRow(row);
                    }
                    else
                    {
                        row.EndsAt = DateTime.UtcNow.AddDays(30).AddHours(6);
                        Subscriptions.SaveRow(row);
                    }

                    break;
            }

            return Ok();
        }

        public static async Task SetSlotCountAsync(string productId, int slots)
        {
            ProductUpdateOptions options = new ProductUpdateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "slots", slots.ToString() }
                }
            };
            ProductService service = new ProductService(_stripeClient);
            await service.UpdateAsync(productId, options);
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

    #endregion
}
