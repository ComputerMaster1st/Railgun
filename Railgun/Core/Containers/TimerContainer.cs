using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;
using TreeDiagram.Models;

namespace Railgun.Core.Containers
{
    public abstract class TimerContainer : ITimerContainer
    {
        protected readonly IServiceProvider _services;
        protected readonly IDiscordClient _client;
        protected readonly Log _log;
        protected Timer _timer = null;

        public ITreeTimer Data { get; }
        public bool IsCompleted { get; protected set; } = false;
        public bool HasCrashed { get; protected set; } = false;

        protected TimerContainer(IServiceProvider services, ITreeTimer data)
        {
            _services = services;
            Data = data;
            _client = _services.GetService<IDiscordClient>();
            _log = _services.GetService<Log>();
        }

        protected abstract Task RunAsync();

        protected abstract void DeleteData();

        public void StartTimer(double ms)
        {
            _timer = new Timer(ms)
            {
                AutoReset = false
            };

            _timer.Elapsed += (s, a) => RunAsync().GetAwaiter();
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public Task ExecuteOverrideAsync() => RunAsync();

        protected void Dispose()
        {
            if (_timer != null) _timer.Dispose();

            DeleteData();
        }
    }
}
