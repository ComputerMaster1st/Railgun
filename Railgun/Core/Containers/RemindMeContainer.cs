using System;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Core.Containers
{
    public class RemindMeContainer
    {
        private readonly IServiceProvider _services;
        private readonly IDiscordClient _client;
        private readonly Log _log;

        public TimerRemindMe Data { get; }
        public bool IsCompleted { get; private set; } = false;
        public bool HasCrashed { get; private set; } = false;

        public RemindMeContainer(IServiceProvider services, TimerRemindMe data) {
            _services = services;
            Data = data;

            _client = _services.GetService<IDiscordClient>();
            _log = _services.GetService<Log>();
        }

        public void StartTimer(double ms) {
            Data.Timer = new Timer(ms);
            Data.Timer.AutoReset = false;
            Data.Timer.Elapsed += async (s, a) => await RemindMeElapsed();
            Data.Timer.Start();
        }

        public void StopTimer() {
            Data.Timer.Stop();
            Data.Timer.Dispose();
        }

        public Task ExecuteOverrideAsync() => RemindMeElapsed();

        private void Dispose() {
            if (Data.Timer != null) {
                Data.Timer.Dispose();
                Data.Timer = null;
            }

            using (var scope = _services.CreateScope()) {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerRemindMes.Remove(Data);
            }
        }

        private async Task RemindMeElapsed() {
            try {
                var guild = await _client.GetGuildAsync(Data.GuildId);

                if (guild == null) throw new NullReferenceException("RemindMe || Guild doesn't exist!");

                var tc = await guild.GetTextChannelAsync(Data.TextChannelId);
                var user = await guild.GetUserAsync(Data.UserId);

                if (tc == null) throw new NullReferenceException("RemindMe || Guild->TextChannel doesn't exist!");
                else if (user == null) throw new NullReferenceException("RemindMe || Guild->User doesn't exist!");

                var output = new StringBuilder()
                    .AppendFormat("{0}, you asked me to remind you about the following...", user.Mention).AppendLine()
                    .AppendLine()
                    .AppendLine(Data.Message);
                
                await tc.SendMessageAsync(output.ToString());

                IsCompleted = true;

                Dispose();

                await _log.LogToBotLogAsync($"Remind Me {Response.GetSeparator()} Timer ID {Data.Id.ToString()} has completed! Awaiting final cleanup.", BotLogType.TimerManager);
            } catch (NullReferenceException ex) {
                HasCrashed = true;

                Dispose();

                await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Timers", $"Timer ID |{Data.Id.ToString()}| RemindMe Container Exception!", ex));
            } catch (Exception ex) {
                if (Data.Timer != null && !IsCompleted) {
                    StopTimer();
                    StartTimer(TimeSpan.FromSeconds(60).TotalMilliseconds);

                    return;
                } else if (Data.Timer == null && !IsCompleted) {
                    HasCrashed = true;

                    Dispose();
                }

                await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "Timers", $"Timer ID |{Data.Id.ToString()}| RemindMe Container Exception!", ex));
            }
        }
    }
}