using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Discord.Webhook;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Utili.Commands;
using static Utili.Program;
using static Utili.MessageSender;

namespace Utili.Features
{
    internal static class Roslyn
    {
        public static async Task<RoslynResult> EvaluateAsync(string code, RoslynGlobals globals)
        {
            ScriptOptions options = ScriptOptions.Default;

            options = options.WithImports(
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
                "Discord",
                "Discord.WebSocket",
                "Discord.Rest",
                "Discord.Webhook");

            options = options.WithReferences(
                typeof(IDiscordClient).Assembly,
                typeof(DiscordShardedClient).Assembly,
                typeof(DiscordRestClient).Assembly,
                typeof(DiscordWebhookClient).Assembly);

            try
            {
                object result = await CSharpScript.EvaluateAsync(code, options, globals);
                GC.Collect();
                return new RoslynResult(result);
            }
            catch(Exception e)
            {
                GC.Collect();
                return new RoslynResult(e);
            }
        }
    }

    public class RoslynCommands : ModuleBase<SocketCommandContext>
    {
        [Command("Evaluate"), Alias("Eval", "e"), Permission(Perm.BotOwner), Cooldown(5)]
        public async Task Evaluate([Remainder] string code)
        {
            RoslynGlobals globals = new RoslynGlobals(_client, Context, _config);

            RoslynResult result = await Roslyn.EvaluateAsync(code, globals);

            if (result.Success)
            {
                await SendSuccessAsync(Context.Channel, "Success", result.Result.ToString());
            }
            else
            {
                await SendFailureAsync(Context.Channel, "Error", result.Exception.Message);
            }
        }
    }

    internal class RoslynResult
    {
        public bool Success { get; }
        public object Result { get; }
        public Exception Exception { get; }

        public RoslynResult(Exception exception)
        {
            Success = false;
            Exception = exception;
        }

        public RoslynResult(object result)
        {
            Success = true;
            Result = result ?? "";
        }
    }

    public class RoslynGlobals
    {
        public DiscordShardedClient Client { get; }
        public DiscordRestClient Rest => Client.Rest;
        public SocketCommandContext Context { get; }
        public Config Config { get; }

        public RoslynGlobals(DiscordShardedClient client, SocketCommandContext context, Config config)
        {
            Client = client;
            Context = context;
            Config = config;
        }
    }
}
