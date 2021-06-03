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
        public partial class Kick
        {
            [Alias("inactive"), BotPerms(GuildPermission.KickMembers)]
            public class InactivityKickInactive : SystemBase
            {
                [Command]
                public Task ExecuteAsync()
                {
                    var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

                    if (data == null || data.Inactivity == null || data.Inactivity.KickDaysThreshold == 0)
                        return ReplyAsync("Unable to kick inactive users! Either the Inactive Monitor Config has not been generated or the Kick Threshold hasn't been set.");

                    return Task.Factory.StartNew(async () => {
                        var inactiveUsers = (await Context.Guild.GetUsersAsync())
                            .Where((u) => u.RoleIds.Contains(data.Inactivity.InactiveRoleId));

                        foreach (var user in inactiveUsers)
                        {
                            data.Inactivity.Users.RemoveAll((f) => f.UserId == user.Id);

                            await user.KickAsync("Kicked for Inactivity");
                            await Task.Delay(1000);
                        }
                    });
                }
            }
        }
    }
}
