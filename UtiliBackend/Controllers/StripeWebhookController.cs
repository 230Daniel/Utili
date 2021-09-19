using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewDatabase;
using NewDatabase.Entities;
using Stripe;
using Subscription = Stripe.Subscription;

namespace UtiliBackend.Controllers
{
    [IgnoreAntiforgeryToken]
    [Route("stripe/webhook")]
    public class StripeWebhookController : Controller
    {
        private static SemaphoreSlim _semaphore = new(1, 1);
        
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly DatabaseContext _dbContext;
        private readonly IStripeClient _stripeClient;
        
        public StripeWebhookController(ILogger<StripeWebhookController> logger, IConfiguration configuration, DatabaseContext dbContext, StripeClient stripeClient)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _stripeClient = stripeClient;
        }
        
        // Webhooks are retried once an hour for up to 3 days or until a 200 status code is returned.

        [HttpPost]
        public async Task<IActionResult> WebhookAsync()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _configuration["Stripe:WebhookSecret"]);
            
            await _semaphore.WaitAsync();

            try
            {
                _logger.LogInformation("Stripe webhook of type {Type} received", stripeEvent.Type);
                
                switch (stripeEvent.Type)
                {
                    case "customer.subscription.created":
                    case "customer.subscription.updated":
                    case "customer.subscription.deleted":

                        var subscription = stripeEvent.Data.Object as Subscription;

                        var productId = subscription.Items.Data[0].Plan.ProductId;
                        var productService = new ProductService(_stripeClient);
                        var product = await productService.GetAsync(productId);
                        var premiumSlots = int.Parse(product.Metadata["slots"]);

                        var customerDetails = await _dbContext.CustomerDetails.FirstOrDefaultAsync(x => x.CustomerId == subscription.CustomerId);

                        var dbSubscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(x => x.Id == subscription.Id);
                        if (dbSubscription is null)
                        {
                            dbSubscription = new NewDatabase.Entities.Subscription(subscription.Id)
                            {
                                UserId = customerDetails.UserId,
                                Slots = premiumSlots,
                                Status = subscription.Status switch
                                {
                                    "active" => SubscriptionStatus.Active,
                                    "past_due" => SubscriptionStatus.PastDue,
                                    "unpaid" => SubscriptionStatus.Unpaid,
                                    "canceled" => SubscriptionStatus.Canceled,
                                    "incomplete" => SubscriptionStatus.Incomplete,
                                    "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
                                    "trialing" => SubscriptionStatus.Trialing,
                                    _ => throw new ArgumentException($"Unknown value for subscription {subscription.Id} status: {subscription.Status}")
                                },
                                ExpiresAt = subscription.CurrentPeriodEnd.AddHours(2)
                            };
                            
                            _dbContext.Subscriptions.Add(dbSubscription);
                            await _dbContext.SaveChangesAsync();
                            return Ok();
                        }

                        if (stripeEvent.Type == "customer.subscription.created")
                        {
                            // We somehow received the created event after the updated event
                            // We shouldn't update the subscription because it would set its status to Incomplete
                            _logger.LogWarning("Received customer.subscription.created for pre-existing subscription {Id}", subscription.Id);
                            return Ok();
                        }
                        
                        dbSubscription.Status = subscription.Status switch
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
                        dbSubscription.ExpiresAt = subscription.CurrentPeriodEnd.AddHours(2);

                        _dbContext.Subscriptions.Update(dbSubscription);
                        await _dbContext.SaveChangesAsync();
                        break;
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while handling stripe webhook");
                return StatusCode(500);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
