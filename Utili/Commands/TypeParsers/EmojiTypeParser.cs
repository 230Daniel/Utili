using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Qmmands;
using Newtonsoft.Json;

namespace Utili.Commands.TypeParsers
{
    public class EmojiTypeParser : DiscordGuildTypeParser<IEmoji>
    {
        private HashSet<string> _emojis;

        public EmojiTypeParser()
        {
            using StreamReader sr = new("emojiList.json");
            var serializer = new JsonSerializer();
            using JsonTextReader reader = new(sr);
            _emojis = serializer.Deserialize<HashSet<string>>(reader);
        }

        public override ValueTask<TypeParserResult<IEmoji>> ParseAsync(Parameter parameter, string value, DiscordGuildCommandContext context)
        {
            if (LocalCustomEmoji.TryParse(value, out var emoji))
            {
                return context.Guild.Emojis.TryGetValue(emoji.Id, out var guildEmoji) ?
                    Success(guildEmoji) :
                    Failure("The provided custom emoji is not from this guild.");
            }

            return _emojis.Contains(value) ?
                Success(new LocalEmoji(value)) :
                Failure("The provided value is not an emoji");
        }
    }
}
