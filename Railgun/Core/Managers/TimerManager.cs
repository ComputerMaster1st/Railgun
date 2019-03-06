using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Containers;
using Railgun.Core.Logging;
using TreeDiagram;
using TreeDiagram.Interfaces;
using TreeDiagram.Models;

namespace Railgun.Core.Managers
{
    public class TimerManager
    {
        private readonly Timer _masterTimer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
        private readonly IServiceProvider _services;
        private readonly Log _log;
        private bool _initialized;

        public List<ITimerContainer> TimerContainers { get; } = new List<ITimerContainer>();

        public TimerManager(IServiceProvider services) {
            _masterTimer.Elapsed += (s, a) => HouseKeepingAsync().GetAwaiter();
            _masterTimer.AutoReset = true;
            _services = services;
            _log = _services.GetService<Log>();
        }

        public bool CreateAndStartTimer<T>(ITreeTimer data) where T : class, ITimerContainer {
            var remainingTime = data.TimerExpire - DateTime.UtcNow;

            if (!(remainingTime.TotalMinutes < 30) || TimerContainers.Any(find => find.Data.Id == data.Id))
                return false;

            var container = (T)Activator.CreateInstance(typeof(T), _services, data);

            container.StartTimer(remainingTime.TotalMilliseconds);

            TimerContainers.Add(container);

            return true;
        }

        public async Task InitializeAsync() {
            await _log.LogToBotLogAsync($"{(_initialized ? "Re-" : "")}Initializing...", BotLogType.TimerManager);

            if (_initialized) {
                _masterTimer.Stop();
                TimerContainers.ForEach(container => container.StopTimer());
                TimerContainers.Clear();
            }

            var newTimers = 0;
            var crashedTimers = 0;
            var completedTimers = 0;

            using (var scope = _services.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                
                CreateOrOverrideTimers<AssignRoleTimerContainer>(db.TimerAssignRoles, ref newTimers, ref completedTimers,
                    ref crashedTimers);
                CreateOrOverrideTimers<KickUserTimerContainer>(db.TimerKickUsers, ref newTimers, ref completedTimers,
                    ref crashedTimers);
                CreateOrOverrideTimers<RemindMeTimerContainer>(db.TimerRemindMes, ref newTimers, ref completedTimers,
                    ref crashedTimers);
            }

            _masterTimer.Start();

            var output = new StringBuilder()
                .AppendFormat("{0}Initialization Completed!", _initialized ? "Re-" : "").AppendLine()
                .AppendLine()
                .AppendFormat("Timers Executed & Cleaned Up : {0}", completedTimers).AppendLine()
                .AppendFormat("Timers Crashed & Cleaned Up  : {0}", crashedTimers).AppendLine()
                .AppendFormat("Timers Started               : {0}", newTimers);
            
            await _log.LogToBotLogAsync(output.ToString(), BotLogType.TimerManager);

            _initialized = true;
        }

        private async Task HouseKeepingAsync() {
            var newTimers = 0;
            var crashedTimers = 0;
            var completedTimers = 0;

            for (var i = 0; i < TimerContainers.Count; i++) {
                var container = TimerContainers[i];

                if (!container.IsCompleted && !container.HasCrashed) continue;
                if (container.IsCompleted) completedTimers++;
                else crashedTimers++;

                TimerContainers.RemoveAt(i);
                i--;
            }

            using (var scope = _services.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                
                newTimers += db.TimerAssignRoles.Count(CreateAndStartTimer<AssignRoleTimerContainer>);
                newTimers += db.TimerKickUsers.Count(CreateAndStartTimer<KickUserTimerContainer>);
                newTimers += db.TimerRemindMes.Count(CreateAndStartTimer<RemindMeTimerContainer>);
            }

            if (newTimers < 1 && completedTimers < 1 && crashedTimers < 1) return;

            var output = new StringBuilder()
                .AppendLine("Timer Housekeeping Completed!")
                .AppendLine()
                .AppendFormat("Started         : {0}", newTimers).AppendLine()
                .AppendFormat("Already Running : {0}", TimerContainers.Count - newTimers).AppendLine()
                .AppendFormat("Crashed         : {0}", crashedTimers).AppendLine()
                .AppendFormat("Final Cleanup   : {0}", completedTimers + crashedTimers);

            await _log.LogToBotLogAsync(output.ToString(), BotLogType.TimerManager);
        }

        private void CreateOrOverrideTimers<TContainer>(IEnumerable<ITreeTimer> dbSet, ref int newTimers, ref int completedTimers,
            ref int crashedTimers) where TContainer : class, ITimerContainer {
            foreach (var data in dbSet) {
                if (data.TimerExpire < DateTime.UtcNow) {
                    var container = (TContainer)Activator.CreateInstance(typeof(TContainer), _services, data);

                    container.ExecuteOverrideAsync().GetAwaiter();
                    
                    if (container.IsCompleted) completedTimers++;
                    else if (container.HasCrashed) crashedTimers++;

                    continue;
                }
                if (CreateAndStartTimer<TContainer>(data)) newTimers++;
            }
        }
    }
}