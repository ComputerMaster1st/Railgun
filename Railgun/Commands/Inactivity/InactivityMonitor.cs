﻿using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Inactivity
{
    [Alias("inactive-monitor", "inactive", "imon"), UserPerms(GuildPermission.ManageGuild), BotPerms(GuildPermission.ManageRoles)]
    public partial class InactivityMonitor : SystemBase
    {
        [Command("enable")]
        public Task EnableAsync()
        {
            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;
            data.IsEnabled = !data.IsEnabled;
            return ReplyAsync($"Inactivity Monitor has now been turned {Format.Bold(data.IsEnabled ? "On" : "Off")}.");
        }

        [Command("role")]
        public async Task SetRoleAsync(IRole role)
        {
            var self = await Context.Guild.GetCurrentUserAsync();
            var selfHighestRole = self.RoleIds.Select(roleId => Context.Guild.GetRole(roleId))
                .Select(tempRole => tempRole.Position).Concat(new[] {0}).Max();

            if (selfHighestRole < role.Position) await ReplyAsync("Please make sure the inactivity role is in a lower role position than me.");

            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;
            data.InactiveRoleId = role.Id;
            await ReplyAsync($"Inactive Role has been set! ({Format.Bold(role.Name)})");
        }
        
        [Command("role")]
        public Task SetRoleAsync([Remainder] string name)
        {
            var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == name);
            return role == null ? ReplyAsync($"Unable to find role: {Format.Bold(name)}") : SetRoleAsync(role);
        }

        [Command("initialize")]
        public async Task InitializeAsync()
        {
            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;

            if (!data.IsEnabled || data.InactiveRoleId == 0 || data.InactiveDaysThreshold == 0)
            {
                await ReplyAsync("Unable to initialize! Please make sure the Inactivity Monitor is enabled, inactive role is set & inactivity threshold is set.");
                return;
            }

            var output = new StringBuilder()
                .AppendFormat("{0} {1} Initializing Inactivity Monitor...", DateTime.Now.ToString("HH:mm:ss"), SystemUtilities.GetSeparator).AppendLine();
            var response = await ReplyAsync(Format.Code(output.ToString()));
            var users = await Context.Guild.GetUsersAsync();
            var monitoringUsers = 0;
            var alreadyMonitoring = 0;

            output.AppendFormat("{0} {1} {2} Users Found! Preparing Monitor...", DateTime.Now.ToString("HH:mm:ss"), 
                SystemUtilities.GetSeparator, users.Count);
            await response.ModifyAsync((x) => x.Content = Format.Code(output.ToString()));

            foreach (var user in users)
            {
                if (user.IsBot || user.IsWebhook) continue;
                if (Context.Guild.OwnerId == user.Id) continue;
                if (data.Users.Any((u) => u.UserId == user.Id))
                {
                    alreadyMonitoring++;
                    continue;
                }
                if (data.UserWhitelist.Any((u) => u == user.Id)) continue;

                var whitelisted = false;

                if (data.RoleWhitelist.Count > 0)
                {
                    foreach (var role in data.RoleWhitelist)
                    {
                        if (!user.RoleIds.Contains(role)) continue;
                        
                        whitelisted = true;
                        break;
                    }
                }

                if (whitelisted) continue;
                
                data.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
                monitoringUsers++;
            }

            output.AppendFormat("{0} {1} Initialization completed!", DateTime.Now.ToString("HH:mm:ss"), 
                    SystemUtilities.GetSeparator).AppendLine()
                .AppendLine()
                .AppendFormat("Monitoring Users   : {0}", monitoringUsers).AppendLine()
                .AppendFormat("Whitelisted Users  : {0}", users.Count - monitoringUsers).AppendLine()
                .AppendFormat("Already Monitoring : {0}", alreadyMonitoring);
            await response.ModifyAsync((x) => x.Content = Format.Code(output.ToString()));
        }

        [Command("list"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task ListAsync()
        {
            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;
            var users = await Context.Guild.GetUsersAsync();
            var inactiveUsers = users.Count((u) => u.RoleIds.Contains(data.InactiveRoleId));
            var output = new StringBuilder()
                .AppendLine("Inactivity Monitor Report!")
                .AppendLine()
                .AppendFormat("Currently Monitoring      : {0}/{1} users", data.Users.Count, users.Count).AppendLine()
                .AppendFormat("Whitelisted (Users/Roles) : {0}/{1}", data.UserWhitelist.Count, data.RoleWhitelist.Count).AppendLine()
                .AppendFormat("Users Marked As Inactive  : {0}", inactiveUsers).AppendLine()
                .AppendLine();

            if (inactiveUsers < 1)
            {
                await ReplyAsync(Format.Code(output.ToString()));
                return;
            }

            foreach (var user in users)
            {
                var activityContainer = data.Users.FirstOrDefault(u => u.UserId == user.Id);

                output.AppendFormat("Username : {0}#{1} {2} ID : {3} {2} Status : {4} {2} Last Active : {5}",
                    user.Username, user.DiscriminatorValue, SystemUtilities.GetSeparator, user.Id, 
                    !user.RoleIds.Contains(data.InactiveRoleId) ? "Active" : "Inactive"
                    , activityContainer != null 
                        ? activityContainer.LastActive.ToString() 
                        : "UNKNOWN! (Something went wrong!)")
                    .AppendLine();
            }

            await ((ITextChannel)Context.Channel).SendStringAsFileAsync("Inactivity Report", output.ToString());
        }

        [Command("show")]
        public async Task ShowAsync()
        {
            var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Inactivity;

            var whitelistedRoles = new StringBuilder();
            var whitelistedUsers = new StringBuilder();

            if (data.RoleWhitelist.Count < 1) whitelistedRoles.AppendFormat("None");
            else
            {
                var deletedRoles = new List<ulong>();
                
                foreach (var roleId in data.RoleWhitelist)
                {
                    var role = Context.Guild.GetRole(roleId);
                    
                    if (role != null) whitelistedRoles.AppendFormat("{0} {1} ", SystemUtilities.GetSeparator, role.Name);
                    else deletedRoles.Add(roleId);
                }

                foreach (var id in deletedRoles) data.RemoveWhitelistRole(id);
            }
            
            if (data.UserWhitelist.Count < 1) whitelistedUsers.AppendFormat("None");
            else
            {
                var deletedUsers = new List<ulong>();
                
                foreach (var userId in data.UserWhitelist)
                {
                    var user = await Context.Guild.GetUserAsync(userId);
                    
                    if (user != null) whitelistedUsers.AppendFormat("{0} {1}#{2} ", SystemUtilities.GetSeparator, 
                        user.Username, user.DiscriminatorValue);
                    else deletedUsers.Add(userId);
                }

                foreach (var id in deletedUsers) data.RemoveWhitelistUser(id);
            }

            var inactiveRole = Context.Guild.GetRole(data.InactiveRoleId);

            var output = new StringBuilder()
                .AppendLine("Inactivity Monitor Configuration")
                .AppendLine()
                .AppendFormat("Enabled            : {0}", data.IsEnabled ? "Yes" : "No").AppendLine()
                .AppendFormat("Inactive Role      : {0}", inactiveRole != null ? inactiveRole.Name : "Not Set!").AppendLine()
                .AppendFormat("Inactive Threshold : {0} days after last active", data.InactiveDaysThreshold).AppendLine()
                .AppendFormat("Kick Threshold     : {0} days after last active (0 = Not Set!)", data.KickDaysThreshold).AppendLine()
                .AppendFormat("Whitelisted Roles  : {0}", whitelistedRoles.ToString()).AppendLine()
                .AppendFormat("Whitelisted Users  : {0}", whitelistedUsers.ToString()).AppendLine();

            await ReplyAsync(Format.Code(output.ToString()));
        }
        
        [Command("reset")]
        public Task ResetAsync()
        {
            var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

            if (data == null) 
                return ReplyAsync("Inactivity Monitor has no data to reset.");

            data.ResetInactivity();
            return ReplyAsync("Inactivity Monitor has been reset & disabled. All active timers will continue until finished.");
        }
    }
}
