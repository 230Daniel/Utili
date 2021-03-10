using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using static Utili.Program;

namespace Utili
{
    internal static class Monitoring
    {
        private static Timer _timer;
        private static DiscordWebhookClient _webhook;

        public static async void Start()
        {
            RestWebhook webhook = await _rest.GetWebhookAsync(_config.StatusWebhookId);
            _webhook = new DiscordWebhookClient(webhook);

            _timer?.Dispose();
            _timer = new Timer(5000);
            _timer.Elapsed += TimerElapsed;
            _timer.Start();

            await _webhook.SendMessageAsync($"Monitoring started for shards {_config.LowerShardId}-{_config.UpperShardId}");
        }

        private static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (DiscordSocketClient shard in _client.Shards)
            {
                _ = Monitor(shard.ShardId);
            }
        }

        private static async Task Monitor(int shardId)
        {
            try
            {
                for (int i = 0; i < 120; i++)
                {
                    if (_client.GetShard(shardId).ConnectionState == Discord.ConnectionState.Connected) return;
                    await Task.Delay(1000);
                }

                _logger.Log("Monitoring", $"Shard {shardId} has been {_client.GetShard(shardId).ConnectionState} for 2 minutes.", LogSeverity.Crit);
                _logger.Log("Monitoring", "Restarting...", LogSeverity.Crit);
                await _webhook.SendMessageAsync($"Shard {shardId} has been {_client.GetShard(shardId).ConnectionState} for 2 minutes.\nRestarting...");
                await Task.Delay(3000);

                Restart();
            }
            catch(Exception e) { _logger.ReportError("Monitoring", e); }
        }

        private static object _restarting = false;
        public static void Restart()
        {
            lock (_restarting)
            {
                if((bool)_restarting) return;
                _restarting = true;
            }

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"./Restart.sh\"",
                    UseShellExecute = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            Environment.Exit(0);
        }
    }
}
