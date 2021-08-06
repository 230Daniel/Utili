using System.Threading.Tasks;
using Disqord.Bot.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Utili.Implementations
{
    public class MyDiscordBotService : DiscordBotService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        
        public MyDiscordBotService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        
        protected override async ValueTask OnMessageReceived(BotMessageReceivedEventArgs e)
        {
            using var scope = _scopeFactory.CreateScope();
            
        }
    }
}
