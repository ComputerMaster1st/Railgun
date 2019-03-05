using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using System.Linq;
using System.Threading.Tasks;
using TreeDiagram;

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
    }
}
