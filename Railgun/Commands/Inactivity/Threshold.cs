﻿using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Inactivity
{
    public partial class InactivityMonitor
    {
        [Alias("threshold")]
        public class Threshold : SystemBase
        {
            private ServerInactivity GetData(ulong guildId, bool create = false)
            {
                ServerProfile data;

                if (create)
                    data = Context.Database.ServerProfiles.GetOrCreateData(guildId);
                else {
                    data = Context.Database.ServerProfiles.GetData(guildId);

                    if (data == null) 
                        return null;
                }

                if (data.Inactivity == null)
                    if (create)
                        data.Inactivity = new ServerInactivity();
                
                return data.Inactivity;
            }

            [Command("inactive")]
            public Task SetInactiveAsync(int threshold)
            {
                if (threshold < 0) return ReplyAsync($"Inactivity Threshold can {Format.Bold("NOT")} be below 1!");

                var data = GetData(Context.Guild.Id, true);

                if (threshold == 0)
                {
                    if (data.InactiveDaysThreshold == 0) return ReplyAsync("Auto-Role for inactivity is already turned off.");

                    data.KickDaysThreshold = 0;
                    return ReplyAsync("Auto-Role for inactivity is now turned off.");
                }

                data.InactiveDaysThreshold = threshold;

                return ReplyAsync($"Inactivity Threshold has now been set to {Format.Bold(threshold.ToString())} day(s).");
            }

            [Command("kick"), BotPerms(GuildPermission.KickMembers)]
            public Task SetKickAsync(int threshold)
            {
                if (threshold < 0) return ReplyAsync($"Inactivity Threshold can {Format.Bold("NOT")} be below 1!");

                var data = GetData(Context.Guild.Id, true);

                if (threshold == 0)
                {
                    if (data.KickDaysThreshold == 0) return ReplyAsync("Auto-Kick for inactivity is already turned off.");

                    data.KickDaysThreshold = 0;
                    return ReplyAsync("Auto-Kick for inactivity is now turned off.");
                }
                if (data.InactiveDaysThreshold < 1) return ReplyAsync("Please set the Inactivity Threshold!");
                if (data.InactiveDaysThreshold > threshold) return ReplyAsync(
                    $"Kick Threshold must be set higher than Inactivity Threshold ({Format.Bold(data.InactiveDaysThreshold.ToString())} day(s))!");

                data.KickDaysThreshold = threshold;

                return ReplyAsync($"Auto-Kick for inactivity is now turned on! Kick Threshold has been set to {Format.Bold(threshold.ToString())} days.");
            }
        }
    }
}
