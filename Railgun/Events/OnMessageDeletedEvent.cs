using System.Threading.Tasks;
using Discord.WebSocket;
using Railgun.Core.Attributes;
using Railgun.Utilities;

namespace Railgun.Events
{
    [PreInitialize]
    public class OnMessageDeletedEvent
    {
        private readonly Analytics _analytics;

        public OnMessageDeletedEvent(DiscordShardedClient client, Analytics analytics)
        {
            _analytics = analytics;

            client.MessageDeleted += (oldMessage, channel) => Task.Factory.StartNew(async () => await ExecuteAsync());
        }

        private Task ExecuteAsync()
        {
            _analytics.DeletedMessages++;

            return Task.CompletedTask;
        }
    }
}