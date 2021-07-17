using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database.Data;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Sharding;
using Disqord.Sharding;
using Utili.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;
using Utili.Commands.TypeParsers;

namespace Utili.Implementations
{
    public class MyDiscordBotSharder : DiscordBotSharder
    {
        protected override async ValueTask<bool> BeforeExecutedAsync(DiscordCommandContext context)
        {
            if (!context.GuildId.HasValue || context.Author.IsBot) return false;

            var row = await Core.GetRowAsync(context.GuildId.Value);
            var excluded = row.ExcludedChannels.Contains(context.ChannelId);
            if (!row.EnableCommands) excluded = !excluded;
            return !excluded;
        }

        protected override LocalMessage FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            static string FormatParameter(Parameter parameter)
            {
                var format = "{0}";
                if (parameter.IsMultiple)
                {
                    format = "{0}[]";
                }
                else
                {
                    if (parameter.IsRemainder)
                        format = "{0}…";

                    format = parameter.IsOptional
                        ? $"({format})"
                        : $"[{format}]";
                }

                return string.Format(format, parameter.Name);
            }

            var reason = FormatFailureReason(context, result);
            if (reason == null)
                return null;

            var embed = new LocalEmbed()
                .WithAuthor("Error", "https://i.imgur.com/Sg4663k.png")
                .WithDescription(reason)
                .WithColor(0xb54343);
            if (result is OverloadsFailedResult overloadsFailedResult)
            {
                foreach (var (overload, overloadResult) in overloadsFailedResult.FailedOverloads)
                {
                    var overloadReason = FormatFailureReason(context, overloadResult);
                    if (overloadReason == null)
                        continue;

                    embed.AddField($"Overload: {overload.FullAliases[0]} {string.Join(' ', overload.Parameters.Select(FormatParameter))}", overloadReason);
                }
            }
            else if (result is CommandOnCooldownResult cooldownResult)
            {
                var cooldown = cooldownResult.Cooldowns.OrderBy(x => x.RetryAfter).Last();
                var seconds = (int) Math.Round(cooldown.Item2.TotalSeconds);
                embed.WithDescription($"You're doing that too fast, try again in {seconds} {(seconds == 1 ? "second" : "seconds")}");
                embed.WithFooter($"{cooldown.Item1.BucketType.ToString().Title()} cooldown");
            }
            else if (context.Command != null)
            {
                embed.WithFooter($"{context.Command.FullAliases[0]} {string.Join(' ', context.Command.Parameters.Select(FormatParameter))}");
            }

            return new LocalMessage()
                .AddEmbed(embed)
                .WithAllowedMentions(LocalAllowedMentions.None);
        }
        
        protected override ValueTask AddTypeParsersAsync(CancellationToken cancellationToken = default)
        {
            Commands.AddTypeParser(new EmojiTypeParser());
            Commands.AddTypeParser(new RoleArrayTypeParser());
            return base.AddTypeParsersAsync(cancellationToken);
        }

        public MyDiscordBotSharder(
            IOptions<DiscordBotSharderConfiguration> options, 
            ILogger<MyDiscordBotSharder> logger,
            IServiceProvider services, 
            DiscordClientSharder client) 
            : base(options, 
                logger,
                services, 
                client) { }
    }
}
