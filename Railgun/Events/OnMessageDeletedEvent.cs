using Discord.WebSocket;
using Railgun.Utilities;
using System.Threading.Tasks;

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

        public void Load() => _client.MessageDeleted += (oldMessage, channel) =>
        {
            Task.Run(() => ExecuteAsync()).ConfigureAwait(false);
            return Task.CompletedTask;
        };

        private Task ExecuteAsync()
        {
            _analytics.DeletedMessages++;
            return Task.CompletedTask;
        }
    }
}