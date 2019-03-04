using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Logging;
using System;
using System.Threading.Tasks;

namespace Railgun.Core.Containers
{
    public abstract class TimerContainer : ITimerContainer, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IDiscordClient _client;
        private readonly Log _log;

        public bool IsCompleted { get; private set; } = false;
        public bool HasCrashed { get; private set; } = false;

        protected TimerContainer(IServiceProvider services)
        {
            _services = services;

            _client = _services.GetService<IDiscordClient>();
            _log = _services.GetService<Log>();
        }

        protected abstract Task RunAsync();

        public void StartTimer(double ms)
        {
            throw new NotImplementedException();
        }

        public void StopTimer()
        {
            throw new NotImplementedException();
        }

        public Task ExecuteOverrideAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
