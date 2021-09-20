using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Database;
using Database.Entities;
using Stripe;

namespace UtiliBackend.Services
{
    public class CustomerService
    {
        private static SemaphoreSlim _semaphore = new(1, 1);
        
        private readonly ILogger<CustomerService> _logger;
        private readonly DatabaseContext _dbContext;
        private readonly IStripeClient _stripeClient;

        public CustomerService(ILogger<CustomerService> logger, DatabaseContext dbContext, StripeClient stripeClient)
        {
            _logger = logger;
            _dbContext = dbContext;
            _stripeClient = stripeClient;
        }

        public async Task<string> GetOrCreateCustomerIdAsync(User user)
        {
            await _semaphore.WaitAsync();
            _logger.LogInformation("Getting or creating a customer id for {UserId}", user.UserId);

            try
            {
                var stripeCustomerService = new Stripe.CustomerService(_stripeClient);
                var customerDetails = await _dbContext.CustomerDetails.FirstOrDefaultAsync(x => x.UserId == user.UserId);

                if (customerDetails is null)
                {
                    _logger.LogInformation("Creating a customer for {UserId}", user.UserId);

                    var options = new CustomerCreateOptions
                    {
                        Description = $"User Id: {user.UserId}",
                        Email = user.Email,
                        Metadata = new Dictionary<string, string>
                        {
                            {"user_id", user.UserId.ToString()}
                        }
                    };

                    var customer = await stripeCustomerService.CreateAsync(options);
                    _logger.LogInformation("Created customer {CustomerId} for {UserId}", customer.Id, user.UserId);

                    customerDetails = new CustomerDetails(customer.Id)
                    {
                        UserId = user.UserId
                    };
                    _dbContext.CustomerDetails.Add(customerDetails);
                    await _dbContext.SaveChangesAsync();
                    
                    return customer.Id;
                }
                else
                {
                    _logger.LogInformation("Returned customer id {CustomerId} for {UserId}", customerDetails.CustomerId, user.UserId);
                    return customerDetails.CustomerId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error thrown while getting or creating customer id for user {UserId}", user.UserId);
            }
            finally
            {
                _logger.LogInformation("Released semaphore");
                _semaphore.Release();
            }
            return null;
        }

        public async Task<Customer> GetCustomerAsync(User user)
        {
            if (user is null) return null;
            
            await _semaphore.WaitAsync();
            _logger.LogInformation("Getting a customer for {UserId}", user.UserId);

            try
            {
                var stripeCustomerService = new Stripe.CustomerService(_stripeClient);
                var customerDetails = await _dbContext.CustomerDetails.FirstOrDefaultAsync(x => x.UserId == user.UserId);

                if (customerDetails is null)
                {
                    _logger.LogInformation("No customer found for {UserId}", user.UserId);
                    
                    return null;
                }
                else
                {
                    _logger.LogInformation("Getting customer {CustomerId} for {UserId}", customerDetails.CustomerId, user.UserId);

                    var customer = await stripeCustomerService.GetAsync(customerDetails.CustomerId);
                    return customer;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while getting customer for user {UserId}", user.UserId);
            }
            finally
            {
                _logger.LogInformation("Released semaphore");
                _semaphore.Release();
            }
            return null;
        }
    }
}
