using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili
{
    internal class PingTest
    {
        private Timer _timer;

        public int RestLatency { get; private set; }
        
        public void Start()
        {
            _timer?.Dispose();

            _timer = new Timer(30000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = TestAsync();
        }

        private async Task TestAsync()
        {
            IGuild guild;
            try { guild = _client.GetGuild(_config.SystemGuildId); }
            catch { guild = await _rest.GetGuildAsync(_config.SystemGuildId); }
            if(guild == null) guild = await _rest.GetGuildAsync(_config.SystemGuildId);

            ITextChannel channel = await guild.GetTextChannelAsync(_config.SystemChannelId);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            IUserMessage message = await channel.SendMessageAsync("Test");
            stopwatch.Stop();

            RestLatency = (int)stopwatch.ElapsedMilliseconds;
        }
    }
}
