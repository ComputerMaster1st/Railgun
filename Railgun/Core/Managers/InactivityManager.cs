using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using TreeDiagram;
using TreeDiagram.Models.Server.Inactivity;

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
            List<ServerInactivity> configs = null;

            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                if (!db.ServerInactivities.Any((f) => f.IsEnabled == true)) return;

                configs = new List<ServerInactivity>(db.ServerInactivities.Where(
                    (f) => f.IsEnabled == true 
                    && f.InactiveRoleId != 0 
                    && f.InactiveDaysThreshold != 0));
            }

            if (configs.Count < 1) return;

            foreach (var config in configs)
            {
                // TODO: Check if user is active or whitelisted
                

                // TODO: Check if user already has inactive role
                // TODO: Create timer to execute role assignment.
            }
        }
    }
}
