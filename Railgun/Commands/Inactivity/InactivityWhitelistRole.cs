using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Linq;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        public partial class Whitelist
        {
            [Alias("role")]
            public class InactivityWhitelistRole : SystemBase
            {
                [Command]
                public async Task ExecuteAsync(IRole role)
                {
                    var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                    var data = profile.Inactivity;
                    var users = (await Context.Guild.GetUsersAsync())
                        .Where(f => f.RoleIds.Contains(role.Id));

                    if (data.RoleWhitelist.Any(f => f == role.Id))
                    {
                        data.RemoveWhitelistRole(role.Id);

                        foreach (var user in users)
                            if (data.Users.All(f => f.UserId != user.Id))
                                data.Users.Add(new UserActivityContainer(user.Id));

                        await ReplyAsync("Role removed from whitelisted!");
                    }

                    data.AddWhitelistRole(role.Id);

                    foreach (var user in users)
                        data.Users.RemoveAll(f => f.UserId == user.Id);

                    await ReplyAsync("Role added to whitelist!");
                }

                [Command]
                public Task ExecuteAsync(string name)
                {
                    var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == name);

                    return role == null ? ReplyAsync($"Unable to find role: {Format.Bold(name)}") : ExecuteAsync(role);
                }
            }
        }
    }
}
