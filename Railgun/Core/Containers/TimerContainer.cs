using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Logging;
using System;

namespace Railgun.Core.Containers
{
    public abstract class TimerContainer : ITimerContainer
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
    }
}
