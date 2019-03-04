using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using System;
using System.Text;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Core.Containers
{
    public class RemindMeTimerContainer : TimerContainer
    {
        private readonly TimerRemindMe _data;

        public RemindMeTimerContainer(IServiceProvider services, ITreeTimer data) : base(services, data)
            => _data = (TimerRemindMe)data;

        protected override void DeleteData()
        {
            using (var scope = _services.CreateScope())
            {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerRemindMes.Remove(_data);
            }
        }

        protected override async Task RunAsync()
        {
            try
            {
                var guild = await _client.GetGuildAsync(_data.GuildId);

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

                await _log.LogToBotLogAsync($"Remind Me {Response.GetSeparator()} Timer ID {_data.Id.ToString()} has completed! Awaiting final cleanup.", BotLogType.TimerManager);
            }
            catch (NullReferenceException ex)
            {
                HasCrashed = true;

                Dispose();

                await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Timers", $"Timer ID |{_data.Id.ToString()}| RemindMe Container Exception!", ex));
            }
            catch (Exception ex)
            {
                if (_timer != null && !IsCompleted)
                {
                    StopTimer();
                    StartTimer(TimeSpan.FromSeconds(60).TotalMilliseconds);
                    return;
                }
                if (_timer == null && !IsCompleted)
                {
                    HasCrashed = true;
                    Dispose();
                }

                await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Timers", $"Timer ID |{_data.Id.ToString()}| RemindMe Container Exception!", ex));
            }
        }
    }
}
