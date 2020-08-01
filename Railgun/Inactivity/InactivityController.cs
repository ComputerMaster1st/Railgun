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
using TreeDiagram.Models;
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

                if (!db.ServerProfiles.Any((f) => f.Inactivity.IsEnabled)) return;

                var configs = new List<ServerProfile>(db.ServerProfiles.AsQueryable().Where(
                    (f) => f.Inactivity.IsEnabled 
                           && f.Inactivity.InactiveRoleId != 0 
                           && f.Inactivity.InactiveDaysThreshold != 0));
                
                if (configs.Count < 1) return;
                
                foreach (var config in configs)
                {
                    var guild = await _client.GetGuildAsync(config.Id);
                    var alreadyInactiveUsers = new List<UserActivityContainer>();
                    var badUserIds = new List<ulong>();
                    var data = config.Inactivity;
    
                    foreach (var container in data.Users) {
                        var currentTime = DateTime.Now;
                        
                        if (container.LastActive.AddDays(data.InactiveDaysThreshold) > currentTime) continue;
    
                        var user = await guild.GetUserAsync(container.UserId);
                        if (user == null)
                        {
                            badUserIds.Add(container.UserId);
                            continue;
                        }
    
                        if (user.RoleIds.Contains(data.InactiveRoleId))
                        {
                            alreadyInactiveUsers.Add(container);
                            continue;
                        }

                        var timer = db.TimerAssignRoles.CreateTimer(config.Id, currentTime.AddMinutes(5));
                        timer.UserId = container.UserId;
                        timer.RoleId = data.InactiveRoleId;
    
                        _timerController.CreateAndStartTimer<AssignRoleTimerContainer>(timer);
                    }

                    foreach (var id in badUserIds) data.Users.RemoveAll(u => u.UserId == id); 
                    if (alreadyInactiveUsers.Count < 1) return;
                    if (data.KickDaysThreshold == 0) return;
    
                    foreach (var container in alreadyInactiveUsers)
                    {
                        var currentTime = DateTime.Now;
                        
                        if (container.LastActive.AddDays(data.KickDaysThreshold) > currentTime) continue;

                        var timer = db.TimerKickUsers.CreateTimer(config.Id, currentTime.AddMinutes(5));
                        timer.UserId = container.UserId;
    
                        _timerController.CreateAndStartTimer<KickUserTimerContainer>(timer);
                    }
                }
            }
        }
    }
}