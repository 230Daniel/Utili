using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Test
{
    class Feature
    {
        private Timer _timer;
        public void Start()
        {
            if(_timer != null) _timer.Dispose();
            _timer = new Timer(1000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Timer elapsed");
        }
    }
}
