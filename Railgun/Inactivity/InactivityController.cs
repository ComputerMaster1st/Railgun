using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Timers;
using Railgun.Timers.Containers;
using TreeDiagram;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.SubModels;

namespace Railgun.Inactivity
{
    public class InactivityController
    {
        private readonly IDiscordClient _client;
        private readonly TimerController _timerController;
        private readonly IServiceProvider _services;
        private readonly Timer _timer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds) {AutoReset = true};
        private bool _initialized;

        public InactivityController(IDiscordClient client, TimerController controller, IServiceProvider services)
        {
            _client = client;
            _timerController = controller;
            _services = services;

            _timer.Elapsed += (s, a) => RunAsync().GetAwaiter();
        }

        public void Initialize()
        {
            if (_initialized) _timer.Stop();
            
            _timer.Start();
            _initialized = true;
        }

        private async Task RunAsync()
        {
            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                if (!db.ServerInactivities.Any((f) => f.IsEnabled)) return;

                var configs = new List<ServerInactivity>(db.ServerInactivities.Where(
                    (f) => f.IsEnabled 
                           && f.InactiveRoleId != 0 
                           && f.InactiveDaysThreshold != 0));
                
                if (configs.Count < 1) return;
                
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

                        var timer = db.TimerAssignRoles.CreateTimer(config.Id, currentTime.AddMinutes(5));
                        timer.UserId = container.UserId;
                        timer.RoleId = config.InactiveRoleId;
    
                        _timerController.CreateAndStartTimer<AssignRoleTimerContainer>(timer);
                    }
    
                    if (alreadyInactiveUsers.Count < 1) return;
                    if (config.KickDaysThreshold == 0) return;
    
                    foreach (var container in alreadyInactiveUsers)
                    {
                        var currentTime = DateTime.Now;
                        
                        if (container.LastActive.AddDays(config.KickDaysThreshold) > currentTime) continue;

                        var timer = db.TimerKickUsers.CreateTimer(config.Id, currentTime.AddMinutes(5));
                        timer.UserId = container.UserId;
    
                        _timerController.CreateAndStartTimer<KickUserTimerContainer>(timer);
                    }
                }
            }
        }
    }
}