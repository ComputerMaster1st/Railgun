using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Linq;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("role")]
        public class InactivityRole : SystemBase
        {
            [Command]
            public async Task ExecuteAsync(IRole role)
            {
                var self = await Context.Guild.GetCurrentUserAsync();
                var selfHighestRole = self.RoleIds.Select(roleId => Context.Guild.GetRole(roleId))
                    .Select(tempRole => tempRole.Position)
                    .Concat(new[] { 0 })
                    .Max();

                if (selfHighestRole < role.Position) 
                    await ReplyAsync("Please make sure the inactivity role is in a lower role position than me.");

                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Inactivity;

                data.InactiveRoleId = role.Id;

                await ReplyAsync($"Inactive Role has been set! ({Format.Bold(role.Name)})");
            }

            [Command]
            public Task ExecuteAsync([Remainder] string name)
            {
                var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == name);

                return role == null ? ReplyAsync($"Unable to find role: {Format.Bold(name)}") : ExecuteAsync(role);
            }
        }
    }
}
