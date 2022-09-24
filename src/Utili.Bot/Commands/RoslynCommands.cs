using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qmmands;
using Qmmands.Text;
using Utili.Bot.Implementations;

namespace Utili.Bot.Features;

public class RoslynCommands : MyDiscordTextGuildModuleBase
{
    private ILogger<RoslynCommands> _logger;
    private IConfiguration _config;

    public RoslynCommands(ILogger<RoslynCommands> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [TextCommand("evaluate", "eval", "e")]
    [RequireBotOwner]
    public async Task<IResult> EvaluateAsync([Remainder] string code)
    {
        if (Context.Message.Author.Id != _config.GetValue<ulong>("Discord:OwnerId"))
        {
            _logger.LogWarning("The bot owner check allowed a non-owner through - The eval command was not executed");
            return null;
        }

        _logger.LogInformation($"Executing code: {code}");

        var options = ScriptOptions.Default
            .WithImports(
                "System",
                "System.Collections.Generic",
                "System.Text",
                "System.Threading.Tasks",
                "System.Linq",
                "Microsoft.Extensions.DependencyInjection",
                "Microsoft.Extensions.Hosting",
                "Microsoft.Extensions.Logging",
                "Microsoft.Extensions.Configuration",
                "Disqord",
                "Disqord.Rest",
                "Disqord.Gateway",
                "Discord.Bot",
                "Utili.Bot",
                "Utili.Bot.Services",
                "Utili.Bot.Utils",
                "Utili.Bot.Extensions")
            .WithReferences(
                typeof(DiscordClientBase).Assembly,
                typeof(DiscordBotBase).Assembly,
                typeof(Program).Assembly);

        RoslynGlobals globals = new(Context.Services, Context);
        try
        {
            var result = await CSharpScript.EvaluateAsync(code, options, globals);
            _logger.LogInformation($"Roslyn result: {result}");
            if (result is null) return Success("Evaluated result", "null");
            return Success("Evaluated result", result.ToString());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception thrown executing evaluate command");
            return Failure("An exception occurred", e.ToString());
        }
    }
}

public class RoslynGlobals
{
    public IServiceProvider Services { get; }
    public DiscordBotBase Client { get; }
    public IDiscordTextGuildCommandContext Context { get; }

    public RoslynGlobals(IServiceProvider services, IDiscordTextGuildCommandContext context)
    {
        Services = services;
        Client = context.Bot;
        Context = context;
    }
}
