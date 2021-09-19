using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace DatabaseStressTest
{
    public abstract class HostedService : IHostedService
    {
        private CancellationTokenSource _tokenSource;
        private Task _task;
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _tokenSource = new CancellationTokenSource();
            _task = RunAsync(_tokenSource.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _tokenSource.Cancel();
                await _task;
            }
            catch (TaskCanceledException) { }
        }
        
        protected virtual Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
