using Finite.Commands;
using Railgun.Core;
using Railgun.Timers;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("timer-restart")]
        public class RootTimerRestart : SystemBase
        {
            private readonly TimerController _timers;

            public RootTimerRestart(TimerController timers)
                => _timers = timers;

            [Command]
            public Task ExecuteAsync()
            {
                _timers.Initialize();

                return ReplyAsync("Timer Manager Restarted!");
            }
        }
    }
}
