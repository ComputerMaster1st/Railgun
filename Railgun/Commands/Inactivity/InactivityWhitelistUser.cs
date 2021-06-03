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
            [Alias("user")]
            public class InactivityWhitelistUser : SystemBase
            {
                [Command]
                public Task ExecuteAsync(IGuildUser user)
                {
                    if (user.IsBot || user.IsWebhook)
                        return ReplyAsync("This user is a bot! Bots will always be ignored/whitelisted.");

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
            }
        }
    }
}
