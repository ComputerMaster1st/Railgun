using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Inactivity
{
    public partial class Inactivity
    {
        [Alias("reset")]
        public class InactivityReset : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
            {
                var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

                if (data == null)
                    return ReplyAsync("Inactivity Monitor has no data to reset.");

                data.ResetInactivity();

                return ReplyAsync("Inactivity Monitor has been reset & disabled. All active timers will continue until finished.");
            }
        }
    }
}
