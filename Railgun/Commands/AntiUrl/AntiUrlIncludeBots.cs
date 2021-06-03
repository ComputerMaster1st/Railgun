using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiUrl
{
    public partial class AntiUrl
    {
        [Alias("includebots")]
        public class AntiUrlIncludeBots : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Filters.Urls;

                data.IncludeBots = !data.IncludeBots;

                return ReplyAsync($"Anti-Url is now {Format.Bold(data.IncludeBots ? "Monitoring" : "Ignoring")} bots.");
            }
        }
    }
}
