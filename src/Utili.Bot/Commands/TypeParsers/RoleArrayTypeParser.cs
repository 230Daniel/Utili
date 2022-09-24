using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Qmmands;
using Utili.Bot.Extensions;

namespace Utili.Bot.Commands.TypeParsers;

public class RoleArrayTypeParser : DiscordGuildTypeParser<RoleArray>
{
    public override ValueTask<ITypeParserResult<RoleArray>> ParseAsync(IDiscordGuildCommandContext context, IParameter parameter, ReadOnlyMemory<char> value)
    {
        var valueString = value.ToString();

        var singleRole = context.GetGuild().Roles.Values.FirstOrDefault(x => x.Mention == valueString);
        singleRole ??= context.GetGuild().Roles.Values.FirstOrDefault(x => x.Id.ToString() == valueString);
        singleRole ??= context.GetGuild().Roles.Values.FirstOrDefault(x => x.Name == valueString);
        singleRole ??= context.GetGuild().Roles.Values.FirstOrDefault(x => x.Name.ToLower() == valueString.ToLower());

        if (singleRole is not null) return Success(new RoleArray(new[] { singleRole }));

        var seperator = valueString.Contains(',') ? "," : " ";
        var roleStrings = valueString.Split(seperator);

        List<IRole> roles = new();
        foreach (var roleString in roleStrings)
        {
            var roleStringTrimmed = roleString.Trim();
            var role = context.GetGuild().Roles.Values.FirstOrDefault(x => x.Mention == roleStringTrimmed);
            role ??= context.GetGuild().Roles.Values.FirstOrDefault(x => x.Id.ToString() == roleStringTrimmed);
            role ??= context.GetGuild().Roles.Values.FirstOrDefault(x => x.Name == roleStringTrimmed);
            role ??= context.GetGuild().Roles.Values.FirstOrDefault(x => x.Name.ToLower() == roleStringTrimmed.ToLower());

            if (role is null)
                return Failure($"Could not find a role matching '{roleStringTrimmed}'");

            roles.Add(role);
        }

        return Success(new RoleArray(roles.ToArray()));
    }
}

public class RoleArray
{
    public IRole[] Array { get; }

    public RoleArray(IRole[] array)
    {
        Array = array;
    }
}
