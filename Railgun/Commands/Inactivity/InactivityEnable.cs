using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("enable")]
        public class InactivityEnable : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
                var data = profile.Inactivity;

                data.IsEnabled = !data.IsEnabled;
                
                return ReplyAsync($"Inactivity Monitor has now been turned {Format.Bold(data.IsEnabled ? "On" : "Off")}.");
            }
        }
    }
}
