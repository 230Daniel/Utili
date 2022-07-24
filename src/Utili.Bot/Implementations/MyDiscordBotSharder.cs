using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Disqord.Bot.Sharding;
using Disqord.Sharding;
using Utili.Bot.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;
using Qmmands.Default;
using Qmmands.Text;
using Utili.Bot.Commands.TypeParsers;

namespace Utili.Bot.Implementations;

public class MyDiscordBotSharder : DiscordBotSharder
{
    protected override async ValueTask<IResult> OnBeforeExecuted(IDiscordCommandContext context)
    {
        if (!context.GuildId.HasValue) return Results.Failure("Commands must be executed in a server.");
        if (context.Author.IsBot) return Results.Failure("Commands can't be executed by a bot.");

        var config = await context.Services.GetCoreConfigurationAsync(context.GuildId.Value);
        if (config is null) return Results.Success;

        if (config.NonCommandChannels.Contains(context.ChannelId))
            return Results.Failure("Commands are disabled in this channel.");

        if (!config.CommandsEnabled)
            return Results.Failure("Commands are not enabled in this server.");

        return Results.Success;
    }

    protected override bool FormatFailureMessage(IDiscordCommandContext context, LocalMessageBase message, IResult result)
    {
        if (context is IDiscordTextCommandContext textContext)
            return FormatTextFailureMessage(textContext, message, result);

        throw new NotImplementedException("Application commands are not yet supported");
    }

    protected bool FormatTextFailureMessage(IDiscordTextCommandContext context, LocalMessageBase message, IResult result)
    {
        var reason = FormatFailureReason(context, result);
        if (reason == null)
            return false;

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

                embed.AddField($"Overload: {overload.Aliases[0]} {string.Join(' ', overload.Parameters.Select(FormatParameter))}", overloadReason);
            }
        }
        else if (result is CommandRateLimitedResult cooldownResult)
        {
            var cooldown = cooldownResult.RateLimits.MaxBy(x => x.Value);
            var seconds = (int)Math.Round(cooldown.Value.TotalSeconds);
            embed.WithDescription($"You're doing that too fast, try again in {seconds} {(seconds == 1 ? "second" : "seconds")}");
            embed.WithFooter($"{cooldown.Key.BucketType.ToString().Title()} cooldown");
        }
        else if (context.Command != null)
        {
            embed.WithFooter($"{context.Command.Aliases[0]} {string.Join(' ', context.Command.Parameters.Select(FormatParameter))}");
        }

        message.AddEmbed(embed);
        message.WithAllowedMentions(LocalAllowedMentions.None);
        return true;
    }

    private static string FormatParameter(ITextParameter parameter)
    {
        if (parameter is not IPositionalParameter positionalParameter)
            throw new NotImplementedException("Only positional parameters are supported");

        var format = "{0}";

        // TODO: Implement when Quah sorts his shit out, again
        /*if (positionalParameter.IsMultiple)
        {
            format = "{0}[]";
        }
        else
        {
            if (positionalParameter.IsRemainder)
                format = "{0}…";

            format = positionalParameter.IsOptional
                ? $"({format})"
                : $"[{format}]";
        }*/

        return string.Format(format, parameter.Name);
    }

    protected override ValueTask AddTypeParsers(DefaultTypeParserProvider typeParserProvider, CancellationToken cancellationToken)
    {
        typeParserProvider.AddParser(new EmojiTypeParser());
        typeParserProvider.AddParser(new RoleArrayTypeParser());
        return base.AddTypeParsers(typeParserProvider, cancellationToken);
    }

    public MyDiscordBotSharder(
        IOptions<DiscordBotSharderConfiguration> options,
        ILogger<MyDiscordBotSharder> logger,
        IServiceProvider services,
        DiscordClientSharder client)
        : base(options,
            logger,
            services,
            client)
    {
    }
}
