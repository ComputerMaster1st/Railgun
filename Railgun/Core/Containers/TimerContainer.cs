using Discord;
using Railgun.Core.Logging;
using System;

namespace Railgun.Core.Containers
{
    public class TimerContainer : ITimerContainer
    {
        private readonly IServiceProvider _services;
        private readonly IDiscordClient _client;
        private readonly Log _log;

        public bool IsCompleted { get; private set; }
        public bool HasCrashed { get; private set; }
    }
}
