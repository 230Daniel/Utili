using Discord.Commands;
using Discord.WebSocket;
using PostSharp.Aspects;
using PostSharp.Serialization;

namespace Utili.Commands
{
    [PSerializable]
    internal class Permission : OnMethodBoundaryAspect
    {
        private Perm Perm { get; set; }

        public Permission(Perm perm)
        {
            Perm = perm;
            AspectPriority = 10;
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

            return perm switch
            {
                Perm.None => true,
                Perm.ManageMessages => user.GetPermissions(channel).ManageMessages,
                Perm.ManageGuild => user.GuildPermissions.ManageGuild,
                Perm.Owner => guild.Owner.Id == user.Id,
                Perm.BotStaff => false,
                Perm.BotOwner => user.Id == 218613903653863427,
                _ => false,
            };
        }

        public string GetPermissionRequirement(Perm perm)
        {
            return perm switch
            {
                Perm.None => "no permissions",
                Perm.ManageMessages => "the manage messages permission",
                Perm.ManageGuild => "the manage server permission",
                Perm.Owner => "to be the owner of the guild",
                Perm.BotStaff => "to be a trusted bot staff member",
                Perm.BotOwner => "to be the bot owner",
                _ => "",
            };
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
