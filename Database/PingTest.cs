using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MySql.Data.MySqlClient;

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

            _timer = new Timer(35000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = TestAsync();
        }

        private async Task TestAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            MySqlCommand command = Sql.GetCommand("UPDATE Test SET Text = '' WHERE FALSE;");
            command.ExecuteNonQuery();
            stopwatch.Stop();

            await command.Connection.CloseAsync();

            NetworkLatency = (int)stopwatch.ElapsedMilliseconds;

            QueriesPerSecond = Sql.Queries / (DateTime.Now - _lastTest).TotalSeconds;
            Sql.Queries = 0;
            _lastTest = DateTime.Now;
        }
    }
}
