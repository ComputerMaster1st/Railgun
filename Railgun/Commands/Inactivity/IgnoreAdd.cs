using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using System.Linq;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models.Server;

namespace Railgun.Commands.Inactivity
{
    public partial class InactivityMonitor
    {
        public partial class Ignore
        {
            [Alias("add")]
            public class IgnoreAdd : SystemBase
            {
                [Command("user")]
                public Task AddUserAsync(IUser user)
                {
                    var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);

                    if (data.UserWhitelist.Any((f) => f.UserId == user.Id)) return ReplyAsync("User is already whitelisted!");

                    data.UserWhitelist.Add(new UlongUserId(user.Id));
                    return ReplyAsync("User added to whitelist!");
                }

                public Task AddRoleAsync(IRole role)
                {
                    var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);

                    if (data.RoleWhitelist.Any((f) => f.RoleId == role.Id)) return ReplyAsync("Role is already whitelisted!");

                    data.RoleWhitelist.Add(new UlongRoleId(role.Id));
                    return ReplyAsync("Role added to whitelist!");
                }

                public Task AddRoleAsync(string name)
                {
                    var role = Context.Guild.Roles.Where((r) => r.Name == name).FirstOrDefault();
                    if (role == null) return ReplyAsync($"Unable to find role: {Format.Bold(name)}");
                    return AddRoleAsync(role);
                }
            }
        }
    }
}
