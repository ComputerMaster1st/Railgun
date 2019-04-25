using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Enums;
using Railgun.Timers.Containers;
using TreeDiagram;
using TreeDiagram.Interfaces;

namespace Railgun.Timers
{
    public class TimerController
    {
        private readonly IDiscordClient _client;
        private readonly BotLog _botLog;
        private readonly IServiceProvider _services;
        private readonly Timer _masterTimer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);   
        private bool _initialized;

        public List<ITimerContainer> TimerContainers { get; } = new List<ITimerContainer>();

        public TimerController(IDiscordClient client, BotLog botLog, IServiceProvider services)
        {
            _client = client;
            _botLog = botLog;
            _services = services;

            _masterTimer.Elapsed += (s, a) => HouseKeepingAsync().GetAwaiter();
            _masterTimer.AutoReset = true;
        }

        public void Initialize()
        {
            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Timers", $"{(_initialized ? "Re-" : "")}Initializing..."));

            if (_initialized)
            {
                _masterTimer.Stop();
                TimerContainers.ForEach(container => container.StopTimer());
                TimerContainers.Clear();
            }

            var newTimers = 0;
            var expiredTimers = 0;

            using (var scope = _services.CreateScope()) 
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                CreateOrOverrideTimers<AssignRoleTimerContainer>(db.TimerAssignRoles, ref newTimers, ref expiredTimers);
                CreateOrOverrideTimers<KickUserTimerContainer>(db.TimerKickUsers, ref newTimers, ref expiredTimers);
                CreateOrOverrideTimers<RemindMeTimerContainer>(db.TimerRemindMes, ref newTimers, ref expiredTimers);
            }

            _masterTimer.Start();
            _initialized = true;
            var output = new StringBuilder()
                .AppendFormat("{0}Initialization Completed!", _initialized ? "Re-" : "").AppendLine()
                .AppendLine()
                .AppendFormat("Timers Found & Restarted : {0}", expiredTimers).AppendLine()
                .AppendFormat("Timers Started           : {0}", newTimers);

            SystemUtilities.LogToConsoleAndFile(new LogMessage(LogSeverity.Info, "Timers", output.ToString()));
        }

        public bool CreateAndStartTimer<T>(ITreeTimer data) where T : class, ITimerContainer
        {
            var remainingTime = data.TimerExpire - DateTime.UtcNow;

            if (!(remainingTime.TotalMinutes < 30) || TimerContainers.Any(find => find.Data.Id == data.Id))
                return false;

            var container = (T)Activator.CreateInstance(typeof(T), _client, _services, data);

            container.StartTimer(remainingTime.TotalMilliseconds);

            TimerContainers.Add(container);

            return true;
        }

        private void CreateOrOverrideTimers<TContainer>(IEnumerable<ITreeTimer> dbSet, ref int newTimers, ref int expiredTimers) where TContainer : class, ITimerContainer
        {
            foreach (var data in dbSet)
            {
                if (data.TimerExpire < DateTime.UtcNow)
                {
                    var container = (TContainer)Activator.CreateInstance(typeof(TContainer), _client, _services, data);

                    container.StartTimer(TimeSpan.FromMinutes(5).TotalMilliseconds);
                    TimerContainers.Add(container);
                    expiredTimers++;
                    continue;
                }
                if (CreateAndStartTimer<TContainer>(data)) newTimers++;
            }
        }

        private Task HouseKeepingAsync()
        {
            var newTimers = 0;
            var crashedTimers = 0;
            var completedTimers = 0;

            for (var i = 0; i < TimerContainers.Count; i++) 
            {
                var container = TimerContainers[i];

                if (!container.IsCompleted && !container.HasCrashed) continue;
                if (container.IsCompleted) completedTimers++;
                else crashedTimers++;

                TimerContainers.RemoveAt(i);
                i--;
            }

            using (var scope = _services.CreateScope()) 
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                
                newTimers += db.TimerAssignRoles.Count(CreateAndStartTimer<AssignRoleTimerContainer>);
                newTimers += db.TimerKickUsers.Count(CreateAndStartTimer<KickUserTimerContainer>);
                newTimers += db.TimerRemindMes.Count(CreateAndStartTimer<RemindMeTimerContainer>);
            }

            if (newTimers < 1 && completedTimers < 1 && crashedTimers < 1) return Task.CompletedTask;

            var output = new StringBuilder()
                .AppendLine("Timer Housekeeping Completed!")
                .AppendLine()
                .AppendFormat("Started         : {0}", newTimers).AppendLine()
                .AppendFormat("Already Running : {0}", TimerContainers.Count - newTimers).AppendLine()
                .AppendFormat("Crashed         : {0}", crashedTimers).AppendLine()
                .AppendFormat("Final Cleanup   : {0}", completedTimers + crashedTimers);
            _botLog.SendBotLogAsync(BotLogType.TimerManager, output.ToString()).GetAwaiter();

            return Task.CompletedTask;
        }
    }
}