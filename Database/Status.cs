using System;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Database
{
    public static class Status
    {
        private static Timer _timer;
        private static DateTime _lastTest;
        private static object _lockObj = new();

        public static int Latency { get; private set; }
        public static double QueriesPerSecond { get; private set; }

        public static void Start()
        {
            _timer?.Dispose();
            _lastTest = DateTime.Now;

            _timer = new Timer(5000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = TestAsync();
        }

        private static async Task TestAsync()
        {
            Latency = await Sql.PingAsync();

            lock (_lockObj)
            {
                QueriesPerSecond = Sql.Queries / (DateTime.Now - _lastTest).TotalSeconds;
                Sql.Queries = 0;
                _lastTest = DateTime.Now;
            }
        }
    }
}
