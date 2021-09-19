using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DatabaseStressTest
{
    public class StressTestService : BackgroundService
    {
        private readonly ILogger<StressTestService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly List<WorkerEnvironment> _environments;
        
        public StressTestService(ILogger<StressTestService> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _environments = new();
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("Starting");
                
                for (var i = 0; i < 20 && !stoppingToken.IsCancellationRequested; i++)
                {
                    var scope = _scopeFactory.CreateScope();
                    var worker = scope.ServiceProvider.GetService<Worker>();
                    var environment = new WorkerEnvironment(scope, worker.RunAsync(stoppingToken));
                    _environments.Add(environment);
                    _logger.LogInformation("Worker {I} started", i);
                }
                
                _logger.LogInformation("All workers started");
                
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogInformation("Shutting down workers");
                    
                    // The workers use the same CancellationToken, just make sure they all shut down
                    await Task.WhenAll(_environments.Select(x => x.Task));
                    
                    _logger.LogInformation("All workers shut down");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "StressTestService threw an exception");
            }

            _logger.LogInformation("Exiting");
        }
        
        protected class WorkerEnvironment
        {
            public IServiceScope Scope { get; }
            public Task Task { get; }
            
            public WorkerEnvironment(IServiceScope scope, Task task)
            {
                Scope = scope;
                Task = task;
            }
        }
    }
}
