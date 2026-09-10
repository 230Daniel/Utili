using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Qmmands;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands.TypeParsers;

public class EmojiTypeParser : DiscordGuildTypeParser<IEmoji>
{
    private HashSet<string> _emojis;

    public EmojiTypeParser()
    {
        var json = File.ReadAllText("emojiList.json");
        _emojis = JsonSerializer.Deserialize<HashSet<string>>(json);
    }

    public override ValueTask<ITypeParserResult<IEmoji>> ParseAsync(IDiscordGuildCommandContext context, IParameter parameter, ReadOnlyMemory<char> value)
    {
        var valueString = value.ToString();

        if (LocalCustomEmoji.TryParse(valueString, out var emoji))
        {
            return context.GetGuild().Emojis.TryGetValue(emoji.Id.Value, out var guildEmoji) ?
                Success(new(guildEmoji)) :
                Failure("The provided custom emoji is not from this guild.");
        }

        return _emojis.Contains(valueString) ?
            Success(new LocalEmoji(valueString)) :
            Failure("The provided value is not an emoji");
    }
}
