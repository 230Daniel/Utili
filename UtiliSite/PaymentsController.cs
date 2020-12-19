﻿using System;
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
            // For demonstration purposes, we're using the Checkout session to retrieve the customer ID.
            // Typically this is stored alongside the authenticated user in your database.
            string checkoutSessionId = req.SessionId;
            SessionService checkoutService = new SessionService(_stripeClient);
            Session checkoutSession = await checkoutService.GetAsync(checkoutSessionId);

            // This is the URL to which your customer will return after
            // they are done managing billing in the Customer Portal.
            string returnUrl = $"https://{HttpContext.Request.Host}/premium";

            Stripe.BillingPortal.SessionCreateOptions options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = checkoutSession.CustomerId,
                ReturnUrl = returnUrl,
            };
            Stripe.BillingPortal.SessionService service = new Stripe.BillingPortal.SessionService(_stripeClient);
            Stripe.BillingPortal.Session session = await service.CreateAsync(options);

            return Ok(session);
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
                case "checkout.session.completed":
                    // Payment is successful and the subscription is created.
                    // You should provision the subscription.
                    break;
                case "invoice.paid":
                    // Continue to provision the subscription as payments continue to be made.
                    // Store the status in your database and check when a user accesses your service.
                    // This approach helps you avoid hitting rate limits.
                    break;
                case "invoice.payment_failed":
                    // The payment failed or the customer does not have a valid payment method.
                    // The subscription becomes past_due. Notify your customer and send them to the
                    // customer portal to update their payment information.
                    break;
                default:
                    // Unhandled event type
                    break;
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

    #endregion
}
