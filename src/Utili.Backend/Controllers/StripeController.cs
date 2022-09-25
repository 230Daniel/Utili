using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Utili.Backend.Authorisation;
using Utili.Backend.Models.Stripe;
using Utili.Backend.Extensions;
using Utili.Backend.Services;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Utili.Backend.Controllers;

[DiscordAuthorise]
[Route("stripe")]
public class StripeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly Services.CustomerService _customerService;
    private readonly IStripeClient _stripeClient;
    private readonly IsPremiumService _isPremiumService;

    public StripeController(
        IConfiguration configuration,
        Services.CustomerService customerService,
        StripeClient stripeClient,
        IsPremiumService isPremiumService)
    {
        _configuration = configuration;
        _customerService = customerService;
        _stripeClient = stripeClient;
        _isPremiumService = isPremiumService;
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSessionAsync([FromBody] CreateCheckoutSessionModel model)
    {
        if (_isPremiumService.IsFree) return NotFound();

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
        if (_isPremiumService.IsFree) return NotFound();

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
        if (_isPremiumService.IsFree) return NotFound();

        var customer = await _customerService.GetCustomerAsync(HttpContext.GetUser());

        if (customer is not null && !string.IsNullOrEmpty(customer.Currency))
            return Json(customer.Currency);

        return Json(null);
    }
}
