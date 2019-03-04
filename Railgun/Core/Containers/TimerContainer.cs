using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Logging;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Railgun.Core.Containers
{
    public abstract class TimerContainer : ITimerContainer, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IDiscordClient _client;
        private readonly Log _log;
        private Timer _timer = null;

        public bool IsCompleted { get; private set; } = false;
        public bool HasCrashed { get; private set; } = false;

        protected TimerContainer(IServiceProvider services)
        {
            _services = services;

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

        public void Dispose()
        {
            if (_timer != null) _timer.Dispose();

            DeleteData();
        }
    }
}
