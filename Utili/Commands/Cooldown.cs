﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Aspects;
using PostSharp.Serialization;
using Discord.Commands;
namespace Utili.Commands
{
    [PSerializable]
    internal class Cooldown : OnMethodBoundaryAspect
    {
        private static List<CooldownItem> _cooldowns = new List<CooldownItem>();

        private double FreeCooldown { get; set; }
        private double PremiumCooldown { get; set; }

        public Cooldown(double freeCooldown, double premiumCooldown)
        {
            FreeCooldown = freeCooldown;
            PremiumCooldown = premiumCooldown;
        }

        public Cooldown(double cooldown)
        {
            FreeCooldown = cooldown;
            PremiumCooldown = cooldown;
        }
        
        public override void OnEntry(MethodExecutionArgs args)
        {
            string command = $"{args.Method.DeclaringType.FullName}.{args.Method.Name}";

            ModuleBase<SocketCommandContext> moduleBase = (ModuleBase<SocketCommandContext>) args.Instance;
            SocketCommandContext context = moduleBase.Context;
            
            double delay = FreeCooldown;
            CooldownItem cooldownItem = GetCooldown(context.Guild.Id, context.User.Id, command);

            if (cooldownItem != null)
            {
                double retrySeconds = Math.Round((cooldownItem.ExpiryTime - DateTime.Now).TotalSeconds, 1);

                _ = MessageSender.SendFailureAsync(context.Channel, "You're on cooldown",
                    $"This command has a cooldown of {delay} seconds\nPlease wait {retrySeconds}s before trying again");

                args.FlowBehavior = FlowBehavior.Return;
                args.ReturnValue = 1;
                return;
            }

            cooldownItem = new CooldownItem(context.Guild.Id, context.User.Id, command, delay);
            _cooldowns.Add(cooldownItem);
        }

        private CooldownItem GetCooldown(ulong guildId, ulong userId, string command)
        {
            _cooldowns.RemoveAll(x => x.ExpiryTime <= DateTime.Now);

            try
            {
                return _cooldowns.First(x => x.GuildId == guildId && x.UserId == userId && x.Command == command);
            }
            catch
            {
                return null;
            }
        }
    }

    internal class CooldownItem
    {
        public ulong GuildId { get; }
        public ulong UserId { get; }
        public string Command { get; }
        public DateTime ExpiryTime { get; }

        public CooldownItem(ulong guildId, ulong userId, string command, double delay)
        {
            GuildId = guildId;
            UserId = userId;
            Command = command;
            ExpiryTime = DateTime.Now.AddSeconds(delay);
        }
    }
}