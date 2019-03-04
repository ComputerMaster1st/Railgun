using System;
using System.Threading.Tasks;
using System.Timers;

namespace Railgun.Core.Managers
{
    public class InactivityManager
    {
        private readonly Timer _timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        private readonly IServiceProvider _services;

        public InactivityManager(IServiceProvider services) => _services = services;

        public void Intialize()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += (s, a) => RunAsync().GetAwaiter();
            _timer.Start();
        }

        public async Task RunAsync()
        {
            // TODO: Check if any configs are enabled

            // TODO: Grab enabled configs

            // TODO: loop each inactivity config

            // TODO: Check if it has role & inactivity threshold

            // TODO: Execute Role Assignment.
        }
    }
}
