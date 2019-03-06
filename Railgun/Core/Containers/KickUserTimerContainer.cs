using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using TreeDiagram;
using TreeDiagram.Interfaces;
using TreeDiagram.Models;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Core.Containers {
    
    public class KickUserTimerContainer : TimerContainer {
        private readonly TimerKickUser _data;
        
        public override ITreeTimer Data => _data;

        public KickUserTimerContainer(IServiceProvider services, ITreeTimer data) : base(services)
            => _data = (TimerKickUser)data;
        
        protected override void DeleteData() {
            using (var scope = Services.CreateScope()) {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerKickUsers.Remove(_data);
            }
        }
        
        protected override async Task RunAsync() {
            try {
                var guild = await Client.GetGuildAsync(_data.GuildId) ?? throw new NullReferenceException("Guild Not Found!");
                var user = await guild.GetUserAsync(_data.UserId) ?? throw new NullReferenceException("User Not Found!");

                await user.KickAsync("Auto-Kicked for Inactivity");
                
                IsCompleted = true;
                Dispose();
            } catch (NullReferenceException ex) {
                HasCrashed = true;
                Dispose();
                await Log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, 
                    "Timers", $"Timer ID |{_data.Id}| KickUser Container Exception!", ex));
            } catch (Exception ex) {
                if (Timer != null && !IsCompleted) {
                    StopTimer();
                    StartTimer(TimeSpan.FromSeconds(60).TotalMilliseconds);
                    return;
                }
                if (Timer == null && !IsCompleted) {
                    HasCrashed = true;
                    Dispose();
                }

                await Log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Timers", $"Timer ID |{_data.Id}| KickUser Container Exception!", ex));
            }
        }
    }
}