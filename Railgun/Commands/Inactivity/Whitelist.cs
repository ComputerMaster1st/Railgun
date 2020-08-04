using System.Linq;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
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
                if (user.IsBot || user.IsWebhook) return ReplyAsync("This user is a bot! Bots will always be ignored/whitelisted.");

                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Inactivity;

                if (data.UserWhitelist.Any(f => f == user.Id))
                {
                    data.RemoveWhitelistUser(user.Id);
                    data.Users.Add(new UserActivityContainer(user.Id));
                    return ReplyAsync("User removed from whitelist!");
                }

                data.AddWhitelistUser(user.Id);
                data.Users.RemoveAll(f => f.UserId == user.Id);
                return ReplyAsync("User added to whitelist!");
            }
            
            [Command("role")]
            public async Task RoleAsync(IRole role)
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
