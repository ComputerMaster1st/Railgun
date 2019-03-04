using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Containers;
using Railgun.Core.Logging;
using Railgun.Core.Utilities;
using TreeDiagram;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Core.Managers
{
    public class TimerManager
    {
        private readonly Timer _masterTimer = new Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
        private readonly IServiceProvider _services;
        private readonly Log _log;
        private bool _initialized = false;

        public List<ITimerContainer> TimerContainers { get; } = new List<ITimerContainer>();

        public TimerManager(IServiceProvider services) {
            _masterTimer.Elapsed += (s, a) => HouseKeepingAsync().GetAwaiter();
            _masterTimer.AutoReset = true;
            _services = services;
            _log = _services.GetService<Log>();
        }

        public async Task<bool> CreateAndStartTimerAsync(TimerRemindMe data, bool isNew = false) {
            var remainingTime = data.TimerExpire - DateTime.UtcNow;

            if (remainingTime.TotalMinutes < 30 && _remindMeContainers.FirstOrDefault(find => find.Data.Id == data.Id) == null) {
                var container = new RemindMeContainer(_services, data);

                container.StartTimer(remainingTime.TotalMilliseconds);

                _remindMeContainers.Add(container);

                if (isNew) await _log.LogToBotLogAsync($"Remind Me {Response.GetSeparator()} Timer #{data.Id} Created & Started!", BotLogType.TimerManager);

                return true;
            } else if (isNew) {
                await _log.LogToBotLogAsync($"Remind Me {Response.GetSeparator()} Timer #{data.Id} Created!", BotLogType.TimerManager);
                return false;
            } else return false;
        }

        public async Task InitializeAsync() {
            await _log.LogToBotLogAsync($"{(_initialized ? "Re-" : "")}Initializing...", BotLogType.TimerManager);

            if (_initialized) {
                _masterTimer.Stop();
                _remindMeContainers.ForEach(container => container.StopTimer());
                _remindMeContainers.Clear();
            }

            var newTimers = 0;
            var crashedTimers = 0;
            var completedTimers = 0;

            using (var scope = _services.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                foreach (var data in db.TimerRemindMes) {
                    if (data.TimerExpire < DateTime.UtcNow) {
                        var container = new RemindMeContainer(_services, data);

                        await container.ExecuteOverrideAsync();

                        if (container.IsCompleted) completedTimers++;
                        else if (container.HasCrashed) crashedTimers++;

                        continue;
                    } else if (await CreateAndStartRemindMeContainerAsync(data)) newTimers++;
                }
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

            for (var i = 0; i < RemindMeContainerCount; i++) {
                var container = _remindMeContainers[i];

                if (container.IsCompleted || container.HasCrashed) {
                    if (container.IsCompleted) completedTimers++;
                    else crashedTimers++;

                    _remindMeContainers.RemoveAt(i);
                    i--;
                }
            }

            using (var scope = _services.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                foreach (var data in db.TimerRemindMes)
                    if (await CreateAndStartRemindMeContainerAsync(data)) newTimers++;
            }

            if (newTimers < 1 && completedTimers < 1 && crashedTimers < 1) return;

            var output = new StringBuilder()
                .AppendLine("Timer Housekeeping Completed!")
                .AppendLine()
                .AppendFormat("Started         : {0}", newTimers).AppendLine()
                .AppendFormat("Already Running : {0}", RemindMeContainerCount - newTimers).AppendLine()
                .AppendFormat("Crashed/Errored : {0}", crashedTimers).AppendLine()
                .AppendFormat("Final Cleanup   : {0}", completedTimers + crashedTimers);

            await _log.LogToBotLogAsync(output.ToString(), BotLogType.TimerManager);
        }
    }
}