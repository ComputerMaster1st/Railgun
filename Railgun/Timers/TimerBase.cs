using System;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using TreeDiagram.Interfaces;

namespace Railgun.Timers
{
    public abstract class TimerBase : ITimerContainer
    {
        protected readonly IServiceProvider Services;
        protected readonly IDiscordClient Client;
        protected Timer Timer;

        public abstract ITreeTimer Data { get; }
        public bool IsCompleted { get; protected set; }
        public bool HasCrashed { get; protected set; }

        protected TimerBase(IDiscordClient client, IServiceProvider services)
        {
            Services = services;
            Client = client;
        }

        protected abstract void DeleteData();
        
        protected abstract Task RunAsync();

        public void StartTimer(double ms)
        {
            Timer = new Timer(ms) { AutoReset = false };
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
            Timer?.Dispose();
            DeleteData();
        }
    }
}