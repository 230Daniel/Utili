using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Disqord.Gateway;
using Microsoft.Extensions.DependencyInjection;
using Utili.Bot.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qmmands;
using Qmmands.Default;
using Qmmands.Text;
using Utili.Bot.Commands.TypeParsers;

namespace Utili.Bot.Services;

public class UtiliDiscordBot : DiscordBot
{
    private readonly IServiceProvider _services;

    protected override async ValueTask<bool> OnMessage(IGatewayUserMessage message)
    {
        if (message.Author.IsBot || !message.GuildId.HasValue) return false;

        using var scope = _services.CreateScope();
        var config = await scope.GetCoreConfigurationAsync(message.GuildId.Value);
        if (config is null) return true;

        var nonCommandChannel = config.NonCommandChannels.Contains(message.ChannelId);
        if (config.CommandsEnabled) return !nonCommandChannel;
        return nonCommandChannel;
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

        if (positionalParameter.GetTypeInformation().IsEnumerable)
        {
            format = "{0}[]";
        }
        else
        {
            if (positionalParameter.IsRemainder)
                format = "{0}…";

            format = positionalParameter.GetTypeInformation().IsOptional
                ? $"({format})"
                : $"[{format}]";
        }

        return string.Format(format, parameter.Name);
    }

    protected override ValueTask AddTypeParsers(DefaultTypeParserProvider typeParserProvider, CancellationToken cancellationToken)
    {
        typeParserProvider.AddParser(new EmojiTypeParser());
        typeParserProvider.AddParser(new RoleArrayTypeParser());
        return base.AddTypeParsers(typeParserProvider, cancellationToken);
    }

    public UtiliDiscordBot(
        IOptions<DiscordBotConfiguration> options,
        ILogger<UtiliDiscordBot> logger,
        IServiceProvider services,
        DiscordClient client)
        : base(options,
            logger,
            services,
            client)
    {
        _services = services;
    }
}
