using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Qmmands;
using Utili.Extensions;

namespace Utili.Features
{
    public class RoslynCommands : DiscordGuildModuleBase
    {
        private IServiceProvider _services;
        private ILogger<RoslynCommands> _logger;
        private IConfiguration _config;

        public RoslynCommands(IServiceProvider services, ILogger<RoslynCommands> logger, IConfiguration config)
        {
            _services = services;
            _logger = logger;
            _config = config;
        }

        [Command("Evaluate", "Eval", "E")]
        [RequireBotOwner]
        public async Task Evaluate([Remainder] string code)
        {
            if (Context.Message.Author.Id != _config.GetValue<ulong>("OwnerId"))
            {
                _logger.LogWarning("The bot owner check allowed a non-owner through - The eval command was not executed");
                return;
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
                    "Utili",
                    "Utili.Services",
                    "Utili.Utils",
                    "Utili.Extensions")
                .WithReferences(
                    typeof(DiscordClientBase).Assembly,
                    typeof(Program).Assembly);

            RoslynGlobals globals = new(_services, Context);
            try
            {
                var result = await CSharpScript.EvaluateAsync(code, options, globals);
                _logger.LogInformation($"Roslyn result: {result}");
                if (result is null) await Context.Channel.SendSuccessAsync("Evaluated result", "null");
                else await Context.Channel.SendSuccessAsync("Evaluated result", result.ToString());
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Exception thrown executing evaluate command");
                await Context.Channel.SendFailureAsync("An exception occurred", e.ToString());
            }
        }
    }

    public class RoslynGlobals
    {
        public IServiceProvider Services { get; }
        public DiscordClientBase Client { get; }
        public DiscordGuildCommandContext Context { get; }

        public RoslynGlobals(IServiceProvider services, DiscordGuildCommandContext context)
        {
            Services = services;
            Client = context.Bot;
            Context = context;
        }
    }
}
