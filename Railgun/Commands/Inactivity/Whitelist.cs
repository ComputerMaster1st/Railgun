using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Inactivity
{
    public partial class InactivityMonitor
    {
        [Alias("whitelist")]
        public class Whitelist : SystemBase
        {
            [Command("user")]
            public Task UserAsync(IGuildUser user)
            {
                var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);

                if (data.UserWhitelist.Any(f => f.UserId == user.Id))
                {
                    data.UserWhitelist.RemoveAll(f => f.UserId == user.Id);
                    data.Users.Add(new UserActivityContainer(user.Id));
                    return ReplyAsync("User removed from whitelist!");
                }

                data.UserWhitelist.Add(new UlongUserId(user.Id));
                data.Users.RemoveAll(f => f.UserId == user.Id);
                return ReplyAsync("User added to whitelist!");
            }
            
            [Command("role")]
            public Task RoleAsync(IRole role)
            {
                var data = Context.Database.ServerInactivities.GetOrCreateData(Context.Guild.Id);
                var users = Context.Guild.GetUsersAsync().GetAwaiter().GetResult()
                    .Where(f => f.RoleIds.Contains(role.Id));

                if (data.RoleWhitelist.Any(f => f.RoleId == role.Id))
                {
                    data.RoleWhitelist.RemoveAll(f => f.RoleId == role.Id);

                    foreach (var user in users)
                        if (data.Users.All(f => f.UserId != user.Id)) 
                            data.Users.Add(new UserActivityContainer(user.Id));
                    
                    return ReplyAsync("Role removed from whitelisted!");
                }

                data.RoleWhitelist.Add(new UlongRoleId(role.Id));

                foreach (var user in users) data.Users.RemoveAll(f => f.UserId == user.Id);
                
                return ReplyAsync("Role added to whitelist!");
            }

            [Command("role")]
            public Task RoleAsync(string name)
            {
                var role = Context.Guild.Roles.FirstOrDefault(r => r.Name == name);
                return role == null ? ReplyAsync($"Unable to find role: {Format.Bold(name)}") : RoleAsync(role);
            }
        }
    }
}
