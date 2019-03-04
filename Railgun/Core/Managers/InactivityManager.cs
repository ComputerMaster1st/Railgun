using Discord;
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
        private readonly IDiscordClient _client;

        public InactivityManager(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetService<IDiscordClient>();
        }

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
                // TODO: Loop all users & check if active
                foreach (var container in config.Users)
                {
                    if (container.LastActive.AddDays(config.InactiveDaysThreshold) > DateTime.Now) continue;

                }

                // TODO: Check if user already has inactive role. If so, check if can be kicked (if enabled)
                // TODO: Create timer to execute role assignment.
            }
        }
    }
}
