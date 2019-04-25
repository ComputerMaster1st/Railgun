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
    public class KickUserTimerContainer : TimerBase 
    {
        private readonly TimerKickUser _data;
        
        public override ITreeTimer Data => _data;

        public KickUserTimerContainer(IDiscordClient client, IServiceProvider services, ITreeTimer data) : base(client, services)
            => _data = (TimerKickUser)data;
        
        protected override void DeleteData() 
        {
            using (var scope = Services.CreateScope()) 
            {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerKickUsers.Remove(_data);
            }
        }
        
        protected override async Task RunAsync() 
        {
            try 
            {
                var guild = await Client.GetGuildAsync(_data.GuildId) ?? throw new NullReferenceException("Guild Not Found!");
                var user = await guild.GetUserAsync(_data.UserId) ?? throw new NullReferenceException("User Not Found!");

                // TODO: Please refactor this somehow...
                var selfRolePosition = 0;
                var userRolePosition = 0;
                var self = await guild.GetCurrentUserAsync();

                foreach (var roleId in self.RoleIds) 
                {
                    var role = guild.GetRole(roleId);
                    if (role.Permissions.KickMembers && role.Position > selfRolePosition) selfRolePosition = role.Position;
                }

                foreach (var roleId in user.RoleIds) 
                {
                    var role = guild.GetRole(roleId);
                    if (role.Position > userRolePosition) userRolePosition = role.Position;
                }

                if (selfRolePosition > userRolePosition) 
                {
                    await user.KickAsync("Auto-Kicked for Inactivity");
                    IsCompleted = true;
                    Dispose();
                } 
                else 
                {
                    SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, 
                        "Timers", $"Timer ID |{_data.Id}| KickUser Container Permission Error! Guild ID : {guild.Id}"));
                    HasCrashed = true;
                    Dispose();
                }
                // END Refactor
            } 
            catch (NullReferenceException ex)
            {
                HasCrashed = true;
                Dispose();
                SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, 
                    "Timers", $"Timer ID |{_data.Id}| KickUser Container Exception!", ex));
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
                    "Timers", $"Timer ID |{_data.Id}| KickUser Container Exception!", ex));
            }
        }
    }
}