using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiUrl
{
    public partial class AntiUrl
    {
        [Alias("mode")]
        public class AntiUrlMode : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Filters.Urls;

                data.DenyMode = !data.DenyMode;

                if (!data.IsEnabled) 
                    data.IsEnabled = true;

                return ReplyAsync(string.Format("Switched Anti-Url Mode to {0}. {1} all urls except listed.",
                    data.DenyMode ? Format.Bold("Deny") : Format.Bold("Allow"),
                    data.DenyMode ? "Deny" : "Allow"));
            }
        }
    }
}
