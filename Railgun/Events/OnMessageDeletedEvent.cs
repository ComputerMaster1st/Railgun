using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using Railgun.Utilities;

namespace Railgun.Events
{
    public class OnMessageDeletedEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly Analytics _analytics;

        public OnMessageDeletedEvent(DiscordShardedClient client, Analytics analytics)
        {
            _client = client;
            _analytics = analytics;
        }

        public void Load() => _client.MessageDeleted += (oldMessage, channel) => Task.Factory.StartNew(() => ExecuteAsync());

        private Task ExecuteAsync()
        {
            _analytics.UpdatedMessages++;
            return Task.CompletedTask;
        }
    }
}