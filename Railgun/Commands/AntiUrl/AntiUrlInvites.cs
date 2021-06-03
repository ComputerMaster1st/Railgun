using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiUrl
{
    public partial class AntiUrl
    {
        [Alias("invites")]
        public class AntiUrlInvites : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Filters.Urls;

                data.BlockServerInvites = !data.BlockServerInvites;

                return ReplyAsync($"Anti-Url is now {Format.Bold(data.BlockServerInvites ? "Blocking" : "Allowing")} server invites.");
            }
        }
    }
}
