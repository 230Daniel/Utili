using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Utili.Bot.Utils;

public class Scheduler<TKey> where TKey : IEquatable<TKey>
{
    public event OnSchedulerCallbackEventHandler Callback;

    private List<Job> _jobs = new();
    private Job _currentJob;
    private CancellationTokenSource _cts;

    public Scheduler()
    {
        _cts = new CancellationTokenSource();
    }

    public void Start()
    {
        _ = Main();
    }

    public void Schedule(TKey key, DateTime activateAt)
    {
        lock (_jobs)
        {
            _jobs.Add(new Job(key, activateAt));
        }

        if (_currentJob is null || activateAt < _currentJob.ActivateAt)
            CancelToken();
    }

    public void Cancel(TKey key)
    {
        lock (_jobs)
        {
            _jobs.RemoveAll(x => x.Key.Equals(key));
        }

        if (_currentJob is not null && key.Equals(_currentJob.Key))
            CancelToken();
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
            try
            {
                lock (_jobs)
                {
                    _currentJob = _jobs.MinBy(x => x.ActivateAt);
                }

                var delay = _currentJob is null
                    ? Timeout.InfiniteTimeSpan
                    : _currentJob.ActivateAt - DateTime.UtcNow;

                await Task.Delay(delay, _cts.Token);

                lock (_jobs)
                {
                    _jobs.Remove(_currentJob);
                }

                _ = Callback.Invoke(_currentJob.Key);
            }
            catch (TaskCanceledException) { }
        }
    }

    private record Job(TKey Key, DateTime ActivateAt);

    public delegate Task OnSchedulerCallbackEventHandler(TKey key);
}
