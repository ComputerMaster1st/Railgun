using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiCaps
{
    public partial class AntiCaps
    {
        [Alias("includebots")]
        public class AntiCapsIncludeBots : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Filters.Caps;

                data.IncludeBots = !data.IncludeBots;

                return ReplyAsync($"Anti-Caps is now {Format.Bold(data.IsEnabled ? "Monitoring" : "Ignoring")} bots.");
            }
        }
    }
}
