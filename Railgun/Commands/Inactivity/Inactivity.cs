using Discord;
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
    public partial class Inactivity : SystemBase
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
    }
}
