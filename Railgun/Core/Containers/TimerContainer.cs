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
        protected readonly IServiceProvider Services;
        protected readonly IDiscordClient Client;
        protected readonly Log Log;
        protected Timer Timer;

        public ITreeTimer Data { get; }
        public bool IsCompleted { get; protected set; }
        public bool HasCrashed { get; protected set; }

        protected TimerContainer(IServiceProvider services, ITreeTimer data)
        {
            Services = services;
            Data = data;
            Client = Services.GetService<IDiscordClient>();
            Log = Services.GetService<Log>();
        }

        protected abstract Task RunAsync();

        protected abstract void DeleteData();

        public void StartTimer(double ms)
        {
            Timer = new Timer(ms)
            {
                AutoReset = false
            };

            Timer.Elapsed += (s, a) => RunAsync().GetAwaiter();
            Timer.Start();
        }

        public void StopTimer()
        {
            Timer.Stop();
            Timer.Dispose();
        }

        public Task ExecuteOverrideAsync() => RunAsync();

        protected void Dispose()
        {
            if (Timer != null) Timer.Dispose();

            DeleteData();
        }
    }
}
