using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data.MySqlClient;
using Timer = System.Timers.Timer;

namespace Database
{
    public class PingTest
    {
        private Timer _timer;
        private DateTime _lastTest;

        public int NetworkLatency { get; private set; }
        public double QueriesPerSecond { get; private set; }

        public void Start()
        {
            _timer?.Dispose();
            _lastTest = DateTime.Now;

            _timer = new Timer(5000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = TestAsync();
        }

        private async Task TestAsync()
        {
            NetworkLatency = await Sql.PingAsync();

            QueriesPerSecond = Sql.Queries / (DateTime.Now - _lastTest).TotalSeconds;
            Sql.Queries = 0;
            _lastTest = DateTime.Now;
        }
    }
}
