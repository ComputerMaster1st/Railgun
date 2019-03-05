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
                    var inactiveUsers = (await Context.Guild.GetUsersAsync()).Where((u) => u.RoleIds.Contains(data.InactiveRoleId));
                    var self = await Context.Guild.GetCurrentUserAsync();
                    IInviteMetadata invite = null;

                    if (self.GuildPermissions.ManageGuild) invite = (await Context.Guild.GetInvitesAsync()).FirstOrDefault();
                    
                    foreach (var user in inactiveUsers)
                    {
                        data.Users.RemoveAll((f) => f.UserId == user.Id);
                        
                        if (data.SendInvite && invite != null)
                        {
                            try
                            {
                                var dm = await user.GetOrCreateDMChannelAsync();
                                await dm.SendMessageAsync($"You have been kicked from {Format.Bold(Context.Guild.Name)} due to {Format.Bold("Inactivity")}. If you wish to return, please use the following invite link, {invite.Url}.");
                            } catch
                            {
                                // Ignore
                            }
                        }
                        
                        await user.KickAsync("Kicked for Inactivity");
                        await Task.Delay(1000);
                    }
                });
            }
        }
    }
}
