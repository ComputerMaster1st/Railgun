using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using TreeDiagram;
using TreeDiagram.Interfaces;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Timers.Containers
{
    public class AssignRoleTimerContainer : TimerBase
    {
        private readonly TimerAssignRole _data;
        
        public override ITreeTimer Data => _data;

        public AssignRoleTimerContainer(IDiscordClient client, IServiceProvider services, ITreeTimer data) : base(client, services)
            => _data = (TimerAssignRole)data;
        
        protected override void DeleteData()
        {
            using (var scope = Services.CreateScope())
            {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerAssignRoles.Remove(_data);
            }
        }
        
        protected override async Task RunAsync() 
        {
            try 
            {
                var guild = await Client.GetGuildAsync(_data.GuildId) ?? throw new NullReferenceException("Guild Not Found!");
                var user = await guild.GetUserAsync(_data.UserId) ?? throw new NullReferenceException("User Not Found!");
                var role = guild.GetRole(_data.RoleId) ?? throw new NullReferenceException("Role Not Found!");

                await user.AddRoleAsync(role);
                
                IsCompleted = true;
                Dispose();
            } 
            catch (NullReferenceException ex) 
            {
                HasCrashed = true;
                Dispose();
                SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, 
                    "Timers", $"Timer ID |{_data.Id}| AssignRole Container Exception!", ex));
            } 
            catch (Exception ex) 
            {
                if (Timer != null && !IsCompleted) 
                {
                    StopTimer();
                    StartTimer(TimeSpan.FromSeconds(60).TotalMilliseconds);
                    return;
                }
                if (Timer == null && !IsCompleted) 
                {
                    HasCrashed = true;
                    Dispose();
                }

                SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, 
                    "Timers", $"Timer ID |{_data.Id}| RemindMe Container Exception!", ex));
            }
        }
    }
}