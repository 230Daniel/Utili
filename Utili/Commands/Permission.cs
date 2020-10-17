using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Serialization;
using Discord.Commands;
using Discord.WebSocket;

namespace Utili.Commands
{
    [PSerializable]
    internal class Permission : OnMethodBoundaryAspect
    {
        private Perm Perm { get; set; }

        public Permission(Perm perm)
        {
            Perm = perm;
        }
        
        public override void OnEntry(MethodExecutionArgs args)
        {
            ModuleBase<SocketCommandContext> moduleBase = (ModuleBase<SocketCommandContext>) args.Instance;
            SocketCommandContext context = moduleBase.Context;

            if (!HasPermission(context, Perm))
            {
                _ = MessageSender.SendFailureAsync(context.Channel, "Permission denied",
                    $"To run that command, you need {GetPermissionRequirement(Perm)}");

                args.FlowBehavior = FlowBehavior.Return;
                args.ReturnValue = 1;
            }
        }

        public bool HasPermission(SocketCommandContext context, Perm perm)
        {
            SocketGuild guild = context.Guild;
            SocketGuildUser user = context.User as SocketGuildUser;
            SocketGuildChannel channel = context.Channel as SocketGuildChannel;

            switch (perm)
            {
                case Perm.None:

                    return true;

                case Perm.ManageMessages:

                    return user.GetPermissions(channel).ManageMessages;

                case Perm.ManageGuild:

                    return user.GuildPermissions.ManageGuild;

                case Perm.Owner:

                    return guild.Owner.Id == user.Id;

                case Perm.BotStaff:

                    return false;

                case Perm.BotOwner:

                    return user.Id == 218613903653863427;

                default:

                    return false;
            }
        }

        public string GetPermissionRequirement(Perm perm)
        {
            switch (perm)
            {
                case Perm.None:

                    return "no permissions";
                
                case Perm.ManageMessages:

                    return "the manage messages permission";

                case Perm.ManageGuild:

                    return "the manage server permission";

                case Perm.Owner:

                    return "to be the owner of the guild";

                case Perm.BotStaff:

                    return "to be a trusted bot staff member";

                case Perm.BotOwner:

                    return "to be the bot owner";

                default:

                    return "";
            }
        }
    }

    public enum Perm
    {
        None,
        ManageMessages,
        ManageGuild,
        Owner,
        BotStaff,
        BotOwner
    }
}
