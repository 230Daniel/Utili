using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using Utili.Backend.Authorisation;
using Utili.Backend.Models.Stripe;
using Utili.Backend.Extensions;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Utili.Backend.Controllers
{
    [DiscordAuthorise]
    [Route("stripe")]
    public class StripeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Services.CustomerService _customerService;
        private readonly IStripeClient _stripeClient;

        public StripeController(IConfiguration configuration, Services.CustomerService customerService, StripeClient stripeClient)
        {
            _configuration = configuration;
            _customerService = customerService;
            _stripeClient = stripeClient;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSessionAsync([FromBody] CreateCheckoutSessionModel model)
        {
            var customerId = await _customerService.GetOrCreateCustomerIdAsync(HttpContext.GetUser());
            if (customerId is null) throw new Exception("Customer ID was null");

            var options = new SessionCreateOptions
            {
                SuccessUrl = $"{_configuration["Frontend:Origin"]}/premium/thankyou",
                CancelUrl = $"{_configuration["Frontend:Origin"]}/premium",
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                Mode = "subscription",
                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price = model.PriceId,
                        Quantity = 1
                    }
                },
                Customer = customerId,
                AllowPromotionCodes = true
            };

            var sessionService = new SessionService(_stripeClient);
            var session = await sessionService.CreateAsync(options);

            return Json(new CheckoutSessionModel
            {
                SessionId = session.Id
            });
        }

        [HttpGet("customer-portal")]
        public async Task<IActionResult> CustomerPortalAsync()
        {
            var customerId = await _customerService.GetOrCreateCustomerIdAsync(HttpContext.GetUser());
            if (customerId is null) throw new Exception("Customer ID was null");

            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = $"{_configuration["Frontend:Origin"]}/premium"
            };

            var sessionService = new Stripe.BillingPortal.SessionService(_stripeClient);
            var session = await sessionService.CreateAsync(options);

            return Json(session);
        }

        [HttpGet("currency")]
        public async Task<IActionResult> CurrencyAsync()
        {
            var customer = await _customerService.GetCustomerAsync(HttpContext.GetUser());

            if (customer is not null && !string.IsNullOrEmpty(customer.Currency))
            {
                return Json(new CustomerCurrencyModel
                {
                    Locked = true,
                    Currency = customer.Currency
                });
            }

            var currency = await GetCustomerCurrencyByIpAsync();

            return Json(new CustomerCurrencyModel
            {
                Locked = false,
                Currency = currency
            });
        }

        private async Task<string> GetCustomerCurrencyByIpAsync()
        {
            var gbp = new[]
            {
                "GB", "IM", "JE", "GG"
            };
            var eur = new[]
            {
                "AX", "EU", "AD", "AT", "BE", "CY", "EE", "FI", "FR", "TF", "DE", "GR", "GP", "IE", "IT", "LV", "LT",
                "LU", "MT", "GF", "MQ", "YT", "MC", "ME", "NL", "PT", "RE", "BL", "MF", "PM", "SM", "SK", "SI", "ES",
                "VA"
            };
            var usd = new[]
            {
                "US", "AS", "IO", "VG", "BQ", "EC", "SV", "GU", "HT", "MH", "FM", "MP", "PW", "PA", "PR", "TL", "TC",
                "VI", "UM"
            };

            var client = new HttpClient();

            var response = await client.GetStringAsync($"https://ipinfo.io/{HttpContext.Connection.RemoteIpAddress}/json");
            var country = JsonConvert.DeserializeObject<IpInfoModel>(response).Country;

            if (gbp.Contains(country)) return "gbp";
            if (eur.Contains(country)) return "eur";
            if (usd.Contains(country)) return "usd";
            return "gbp";
        }
    }
}
