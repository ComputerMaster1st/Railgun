using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using TreeDiagram;
using TreeDiagram.Interfaces;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Timers.Containers
{
    public class RemindMeTimerContainer : TimerBase
    {
        private readonly TimerRemindMe _data;
        public override ITreeTimer Data => _data;

        public RemindMeTimerContainer(IDiscordClient client, IServiceProvider services, ITreeTimer data) : base(client, services)
            => _data = (TimerRemindMe)data;

        protected override void DeleteData()
        {
            using (var scope = Services.CreateScope())
            {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerRemindMes.Remove(_data);
            }
        }

        protected override async Task RunAsync()
        {
            try
            {
                var guild = await Client.GetGuildAsync(_data.GuildId);

                if (guild == null) throw new NullReferenceException("RemindMe || Guild doesn't exist!");

                var tc = await guild.GetTextChannelAsync(_data.TextChannelId);
                var user = await guild.GetUserAsync(_data.UserId);

                if (tc == null) throw new NullReferenceException("RemindMe || Guild->TextChannel doesn't exist!");
                if (user == null) throw new NullReferenceException("RemindMe || Guild->User doesn't exist!");

                var output = new StringBuilder()
                    .AppendFormat("{0}, you asked me to remind you about the following...", user.Mention).AppendLine()
                    .AppendLine()
                    .AppendLine(_data.Message);

                await tc.SendMessageAsync(output.ToString());

                IsCompleted = true;
                Dispose();
            }
            catch (NullReferenceException ex)
            {
                HasCrashed = true;
                Dispose();
                SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Warning, 
                    "Timers", $"Timer ID |{_data.Id}| RemindMe Container Exception!", ex));
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