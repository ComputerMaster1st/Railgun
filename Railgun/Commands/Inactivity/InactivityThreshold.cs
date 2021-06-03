using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("threshold")]
        public partial class Threshold : SystemBase
        {
            [Command("inactive")]
            public Task SetInactiveAsync(int threshold)
            {
                if (threshold < 0) return ReplyAsync($"Inactivity Threshold can {Format.Bold("NOT")} be below 1!");

                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Inactivity;

                if (threshold == 0)
                {
                    if (data.InactiveDaysThreshold == 0) return ReplyAsync("Auto-Role for inactivity is already turned off.");

                    data.KickDaysThreshold = 0;
                    return ReplyAsync("Auto-Role for inactivity is now turned off.");
                }

                data.InactiveDaysThreshold = threshold;

                return ReplyAsync($"Inactivity Threshold has now been set to {Format.Bold(threshold.ToString())} day(s).");
            }
        }
    }
}
