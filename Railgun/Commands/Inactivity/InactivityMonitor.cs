using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models.Server.Inactivity;

namespace Railgun.Commands.Inactivity
{
    [Alias("inactive-monitor", "inactive", "imon"), UserPerms(GuildPermission.ManageGuild), BotPerms(GuildPermission.ManageRoles)]
    public partial class InactivityMonitor : SystemBase
    {
        [Command("enable")]
        public Task EnableAsync()
        {
            var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);
            data.IsEnabled = !data.IsEnabled;
            return ReplyAsync($"Inactivity Monitor has now been turned {Format.Bold(data.IsEnabled ? "On" : "Off")}.");
        }

        [Command("role")]
        public Task SetRoleAsync(string name)
        {
            var role = Context.Guild.Roles.Where((r) => r.Name == name).FirstOrDefault();
            if (role == null) return ReplyAsync($"Unable to find role: {Format.Bold(name)}");
            return SetRoleAsync(role);
        }

        [Command("role")]
        public Task SetRoleAsync(IRole role)
        {
            var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);
            data.InactiveRoleId = role.Id;
            return ReplyAsync($"Inactive Role has been set! ({Format.Bold(role.Name)})");
        }

        [Command("sendinvite"), BotPerms(GuildPermission.ManageGuild)]
        public Task SendInviteAsync()
        {
            var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);
            data.SendInvite = !data.SendInvite;
            return ReplyAsync($"Send Invites has now been turned {Format.Bold(data.IsEnabled ? "On" : "Off")}.");
        }

        [Command("initialize")]
        public async Task InitializeAsync()
        {
            var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);

            if (!data.IsEnabled || data.InactiveRoleId == 0 || data.InactiveDaysThreshold == 0)
            {
                await ReplyAsync("Unable to initialize! Please make sure the Inactivity Monitor is enabled, inactive role is set & inactivity threshold is set.");
                return;
            }

            var output = new StringBuilder()
                .AppendFormat("{0} {1} Initializing Inactivity Monitor...", DateTime.Now.ToString("HH:mm:ss"), Response.GetSeparator()).AppendLine();
            var response = await ReplyAsync(Format.Code(output.ToString()));
            var users = await Context.Guild.GetUsersAsync();
            var monitoringUsers = 0;
            var alreadyMonitoring = 0;

            output.AppendFormat("{0} {1} {2} Users Found! Preparing Monitor...", DateTime.Now.ToString("HH:mm:ss"), Response.GetSeparator(), users.Count);
            await response.ModifyAsync((x) => x.Content = Format.Code(output.ToString()));

            foreach (var user in users)
            {
                if (data.Users.Any((u) => u.UserId == user.Id))
                {
                    alreadyMonitoring++;
                    continue;
                }
                if (data.UserWhitelist.Any((u) => u.UserId == user.Id)) continue;
                if (data.RoleWhitelist.Count > 0) foreach (var roleId in data.RoleWhitelist) if (user.RoleIds.Contains(roleId.RoleId)) continue;
                
                data.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
                monitoringUsers++;
            }

            output.AppendFormat("{0} {1} Initialization completed!", DateTime.Now.ToString("HH:mm:ss"), Response.GetSeparator()).AppendLine()
                .AppendLine()
                .AppendFormat("Monitoring Users   : {0}", monitoringUsers).AppendLine()
                .AppendFormat("Whitelisted Users  : {0}", users.Count - monitoringUsers).AppendLine()
                .AppendFormat("Already Monitoring : {0}", alreadyMonitoring);
            await response.ModifyAsync((x) => x.Content = Format.Code(output.ToString()));
        }

        [Command("list"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task ListAsync()
        {
            var data = Context.Database.ServerInactivities.GetData(Context.Guild.Id);

            if (data == null)
            {
                await ReplyAsync("No Inactivity Monitor Config has been generated!");
                return;
            }

            var users = await Context.Guild.GetUsersAsync();
            var inactiveUsers = users.Where((u) => u.RoleIds.Contains(data.InactiveRoleId));
            var output = new StringBuilder()
                .AppendLine("Inactivity Monitor Report!")
                .AppendLine()
                .AppendFormat("Currently Monitoring      : {0}/{1} users", data.Users.Count, users.Count).AppendLine()
                .AppendFormat("Whitelisted (Users/Roles) : {0}/{1}", data.UserWhitelist.Count, data.RoleWhitelist.Count).AppendLine()
                .AppendFormat("Users Marked As Inactive  : {0}", inactiveUsers.Count()).AppendLine()
                .AppendLine();

            if (inactiveUsers.Count() < 1)
            {
                await ReplyAsync(Format.Code(output.ToString()));
                return;
            }

            foreach (var user in inactiveUsers)
            {
                var activityContainer = data.Users.Where((u) => u.UserId == user.Id).FirstOrDefault();

                output.AppendFormat("Username : {0} {1} ID : {2} {1} Last Active : {3}",
                    user.Username, Response.GetSeparator(), user.Id, activityContainer != null ? activityContainer.LastActive.ToString() : "UNKNOWN! (Something went wrong!)")
                    .AppendLine();
            }

            await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "Inactivity Report", output.ToString());
        }
    }
}
