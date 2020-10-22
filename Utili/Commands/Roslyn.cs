using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.CSharp;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Utili.Commands
{
    public class Roslyn : ModuleBase<SocketCommandContext>
    {
        [Command("Execute"), Alias("Evaluate"), Permission(Perm.BotOwner)]
        public async Task Execute([Remainder] string code)
        {
            throw new NotImplementedException();
        }
    }
}
