using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Utili.Commands.TypeParsers
{
    public class RoleArrayTypeParser : DiscordGuildTypeParser<IRole[]>
    {
        public override ValueTask<TypeParserResult<IRole[]>> ParseAsync(Parameter parameter, string value, DiscordGuildCommandContext context)
        {
            var singleRole = context.Guild.Roles.Values.FirstOrDefault(x => x.Mention == value);
            singleRole ??= context.Guild.Roles.Values.FirstOrDefault(x => x.Id.ToString() == value);
            singleRole ??= context.Guild.Roles.Values.FirstOrDefault(x => x.Name == value);
            singleRole ??= context.Guild.Roles.Values.FirstOrDefault(x => x.Name.ToLower() == value.ToLower());

            if (singleRole is not null) return Success(new[] { singleRole });

            var seperator = value.Contains(",") ? "," : " ";
            var roleStrings = value.Split(seperator);

            List<IRole> roles = new();
            foreach (var roleString in roleStrings)
            {
                var roleStringTrimmed = roleString.Trim();
                var role = context.Guild.Roles.Values.FirstOrDefault(x => x.Mention == roleStringTrimmed);
                role ??= context.Guild.Roles.Values.FirstOrDefault(x => x.Id.ToString() == roleStringTrimmed);
                role ??= context.Guild.Roles.Values.FirstOrDefault(x => x.Name == roleStringTrimmed);
                role ??= context.Guild.Roles.Values.FirstOrDefault(x => x.Name.ToLower() == roleStringTrimmed.ToLower());
                if (role is null)
                    return Failure($"Could not find a role matching '{roleStringTrimmed}'");
                roles.Add(role);
            }

            return Success(roles.ToArray());
        }
    }
}
