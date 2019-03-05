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
            [Alias("remove")]
            public class IgnoreRemove : SystemBase
            {
                [Command("user")]
                public Task RemoveUserAsync(IUser user)
                {
                    var data = Context.Database.ServerInactivities.GetData(Context.Guild.Id);

                    if (data == null) return ReplyAsync("No Inactivity Monitor Config has been generated!");
                    if (!data.UserWhitelist.Any((f) => f.UserId == user.Id)) return ReplyAsync("User was never whitelisted!");

                    data.UserWhitelist.RemoveAll((f) => f.UserId == user.Id);
                    return ReplyAsync("User removed from whitelist!");
                }

                public Task RemoveRoleAsync(IRole role)
                {
                    var data = Context.Database.ServerInactivities.GetData(Context.Guild.Id);

                    if (data == null) return ReplyAsync("No Inactivity Monitor Config has been generated!");
                    if (!data.RoleWhitelist.Any((f) => f.RoleId == role.Id)) return ReplyAsync("Role was never whitelisted!");

                    data.RoleWhitelist.RemoveAll((f) => f.RoleId == role.Id);
                    return ReplyAsync("Role removed from whitelist!");
                }

                public Task RemoveRoleAsync(string name)
                {
                    var role = Context.Guild.Roles.Where((r) => r.Name == name).FirstOrDefault();
                    if (role == null) return ReplyAsync($"Unable to find role: {Format.Bold(name)}");
                    return RemoveRoleAsync(role);
                }
            }
        }
    }
}
