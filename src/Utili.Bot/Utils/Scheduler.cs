using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Utili.Bot.Utils;

public class Scheduler<TKey> where TKey : IEquatable<TKey>
{
    public event OnSchedulerCallbackEventHandler Callback;

    private List<Job> _jobs;
    private Job _currentJob;

    private SemaphoreSlim _semaphore;
    private CancellationTokenSource _cts;

    public Scheduler()
    {
        _jobs = new List<Job>();
        _semaphore = new SemaphoreSlim(1, 1);
        _cts = new CancellationTokenSource();
    }

    public void Start()
    {
        _ = Main();
    }

    public async Task ScheduleAsync(TKey key, DateTime activateAt)
    {
        await _semaphore.WaitAsync();

        try
        {
            _jobs.Add(new Job(key, activateAt));

            if (_currentJob is null || activateAt < _currentJob.ActivateAt)
                CancelToken();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task CancelAsync(TKey key)
    {
        await _semaphore.WaitAsync();

        try
        {
            _jobs.RemoveAll(x => x.Key.Equals(key));

            if (_currentJob is not null && key.Equals(_currentJob.Key))
                CancelToken();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void CancelToken()
    {
        var oldCts = _cts;
        _cts = new CancellationTokenSource();
        oldCts.Cancel();
    }

    private async Task Main()
    {
        while (true)
        {
            await _semaphore.WaitAsync();

            try
            {
                _currentJob = _jobs.MinBy(x => x.ActivateAt);

                var delay = _currentJob is null
                    ? Timeout.InfiniteTimeSpan
                    : _currentJob.ActivateAt - DateTime.UtcNow;

                if (_currentJob is not null && delay < TimeSpan.Zero)
                    delay = TimeSpan.Zero;

                _semaphore.Release();

                await Task.Delay(delay, _cts.Token);

                await _semaphore.WaitAsync();

                var dueJob = _currentJob;
                _jobs.Remove(_currentJob);
                _currentJob = null;

                _semaphore.Release();

                _ = Callback.Invoke(dueJob.Key);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error with Scheduler\n{ex}");
                throw;
            }
        }
    }

    private record Job(TKey Key, DateTime ActivateAt);

    public delegate Task OnSchedulerCallbackEventHandler(TKey key);
}
