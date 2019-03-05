using Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Railgun.Core.Containers;
using TreeDiagram;
using TreeDiagram.Models.Server.Inactivity;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Core.Managers
{
    public class InactivityManager {
        private readonly Timer _timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds);
        private readonly IServiceProvider _services;
        private readonly IDiscordClient _client;
        private readonly TimerManager _timerManager;

        public InactivityManager(IServiceProvider services)
        {
            _services = services;
            _client = _services.GetService<IDiscordClient>();
            _timerManager = _services.GetService<TimerManager>();
        }

        public void Initialize()
        {
            _timer.AutoReset = true;
            _timer.Elapsed += (s, a) => RunAsync().GetAwaiter();
            _timer.Start();
        }

        private async Task RunAsync() {
            List<ServerInactivity> configs;
            
            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                if (!db.ServerInactivities.Any((f) => f.IsEnabled)) return;

                configs = new List<ServerInactivity>(db.ServerInactivities.Where(
                    (f) => f.IsEnabled 
                           && f.InactiveRoleId != 0 
                           && f.InactiveDaysThreshold != 0));
                
                if (configs.Count < 1) return;
            }
            
            foreach (var config in configs)
            {
                var guild = await _client.GetGuildAsync(config.Id);
                var alreadyInactiveUsers = new List<UserActivityContainer>();

                foreach (var container in config.Users) {
                    var currentTime = DateTime.Now;
                    
                    if (container.LastActive.AddDays(config.InactiveDaysThreshold) > currentTime) continue;

                    var user = await guild.GetUserAsync(container.UserId);

                    if (user.RoleIds.Contains(config.InactiveRoleId))
                    {
                        alreadyInactiveUsers.Add(container);
                        continue;
                    }

                    TimerAssignRole data;
                    
                    using (var scope = _services.CreateScope()) {
                        data = scope.ServiceProvider.GetService<TreeDiagramContext>().TimerAssignRoles
                            .CreateTimer(config.Id, currentTime.AddMinutes(5));
                        data.UserId = container.UserId;
                        data.RoleId = config.InactiveRoleId;
                    }

                    _timerManager.CreateAndStartTimer<AssignRoleTimerContainer>(data);
                }

                if (alreadyInactiveUsers.Count < 1) return;
                if (config.KickDaysThreshold == 0) return;

                foreach (var container in alreadyInactiveUsers)
                {
                    var currentTime = DateTime.Now;
                    
                    if (container.LastActive.AddDays(config.KickDaysThreshold) > currentTime) continue;

                    TimerKickUser data;
                    
                    using (var scope = _services.CreateScope()) {
                        data = scope.ServiceProvider.GetService<TreeDiagramContext>().TimerKickUsers
                            .CreateTimer(config.Id, currentTime.AddMinutes(5));
                        data.UserId = container.UserId;
                    }

                    _timerManager.CreateAndStartTimer<KickUserTimerContainer>(data);
                }
            }
        }
    }
}
