using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Railgun.Utilities;

namespace Railgun.Events
{
    public class OnMessageReceivedEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly Analytics _analytics;
        private readonly List<IOnMessageSubEvent> _subEvents = new List<IOnMessageSubEvent>();

        public OnMessageReceivedEvent(DiscordShardedClient client, Analytics analytics)
        {
            _client = client;
            _analytics = analytics;
        }

        public void Load() {
            _client.MessageReceived += (message) => Task.Factory.StartNew(async () => await ExecuteReceivedAsync(message));
            _client.MessageUpdated += (oldMessage, newMessage, channel) => Task.Factory.StartNew(async () => await ExecuteUpdatedAsync(newMessage));
        }

        public OnMessageReceivedEvent AddSubEvent(IOnMessageSubEvent sEvent)
        {
            _subEvents.Add(sEvent);
            return this;
        }

        private async Task ExecuteReceivedAsync(SocketMessage message)
        {
            _analytics.ReceivedMessages++;
            await ExecuteAsync(message);
        }

        private async Task ExecuteUpdatedAsync(SocketMessage message)
        {
            _analytics.UpdatedMessages++;
            if (message.IsPinned) return;
            await ExecuteAsync(message);          
        }

        private async Task ExecuteAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage) || !(message.Channel is SocketGuildChannel) || string.IsNullOrEmpty(message.Content)) return;

            foreach (var subEvent in _subEvents)
                await subEvent.ExecuteAsync(message);
        }
    }
}