using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.SubModels;

namespace Railgun.Commands.Inactivity
{
    public partial class InactivityMonitor
    {
        [Alias("whitelist")]
        public class Whitelist : SystemBase
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

            [Command("user")]
            public Task UserAsync(IGuildUser user)
            {
                if (user.IsBot || user.IsWebhook) return ReplyAsync("This user is a bot! Bots will always be ignored/whitelisted.");

                var data = GetData(Context.Guild.Id, true);

                if (data.UserWhitelist.Any(f => f == user.Id))
                {
                    data.UserWhitelist.RemoveAll(f => f == user.Id);
                    data.Users.Add(new UserActivityContainer(user.Id));
                    return ReplyAsync("User removed from whitelist!");
                }

                data.UserWhitelist.Add(user.Id);
                data.Users.RemoveAll(f => f.UserId == user.Id);
                return ReplyAsync("User added to whitelist!");
            }
            
            [Command("role")]
            public async Task RoleAsync(IRole role)
            {
                var data = GetData(Context.Guild.Id, true);
                var users = (await Context.Guild.GetUsersAsync())
                    .Where(f => f.RoleIds.Contains(role.Id));

                if (data.RoleWhitelist.Any(f => f == role.Id))
                {
                    data.RoleWhitelist.RemoveAll(f => f == role.Id);

                    foreach (var user in users)
                        if (data.Users.All(f => f.UserId != user.Id)) 
                            data.Users.Add(new UserActivityContainer(user.Id));
                    
                    await ReplyAsync("Role removed from whitelisted!");
                }

                data.RoleWhitelist.Add(role.Id);

                foreach (var user in users) data.Users.RemoveAll(f => f.UserId == user.Id);
                
                await ReplyAsync("Role added to whitelist!");
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
