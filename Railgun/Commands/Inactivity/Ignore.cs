using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands;

namespace Railgun.Commands.Inactivity
{
    public partial class InactivityMonitor
    {
        [Alias("ignore")]
        public partial class Ignore : SystemBase
        {
            [Command]
            public Task RunAsync() => ReplyAsync("What am I ignoring?");
        }
    }
}
