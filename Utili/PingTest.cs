﻿using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using LinuxSystemStats;
using static Utili.Program;

namespace Utili
{
    internal class PingTest
    {
        private Timer _timer;

        public int RestLatency { get; private set; }
        public double CpuPercentage { get; private set; }
        public MemoryInformation Memory { get; private set; }
        
        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(15000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();

            _ = TestAsync();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = TestAsync();
        }

        private async Task TestAsync()
        {
            try
            {
                CpuPercentage = await Stats.GetCurrentCpuUsagePercentageAsync(0);
                Memory = await Stats.GetMemoryInformationAsync();
            }
            catch { }

            IGuild guild;
            try { guild = _client.GetGuild(_config.SystemGuildId); }
            catch { guild = await _rest.GetGuildAsync(_config.SystemGuildId); }
            if(guild == null) guild = await _rest.GetGuildAsync(_config.SystemGuildId);

            ITextChannel channel = await guild.GetTextChannelAsync(_config.SystemChannelId);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await channel.SendMessageAsync("Test");
            stopwatch.Stop();

            RestLatency = (int)stopwatch.ElapsedMilliseconds;
        }
    }
}
