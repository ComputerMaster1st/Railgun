using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using System.Linq;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Inactivity
{
    public partial class InactivityMonitor
    {
        [Alias("kick")]
        public class Kick : SystemBase
        {
            [Command("inactive"), BotPerms(GuildPermission.KickMembers)]
            public Task KickInactiveAsync()
            {
                var data = Context.Database.ServerInactivities.GetData(Context.Guild.Id);

                if (data == null || data.KickDaysThreshold == 0)
                    return ReplyAsync("Unable to kick inactive users! Either the Inactive Monitor Config has not been generated or the Kick Threshold hasn't been set.");

                return Task.Factory.StartNew(async () => {
                    var inactiveUsers = (await Context.Guild.GetUsersAsync())
                        .Where((u) => u.RoleIds.Contains(data.InactiveRoleId));
                    
                    foreach (var user in inactiveUsers)
                    {
                        data.Users.RemoveAll((f) => f.UserId == user.Id);
                        
                        await user.KickAsync("Kicked for Inactivity");
                        await Task.Delay(1000);
                    }
                });
            }
        }
    }
}
