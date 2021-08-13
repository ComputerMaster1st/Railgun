using Discord;
using Discord.WebSocket;
using Railgun.Core;
using Railgun.Core.Configuration;
using System.Threading.Tasks;

namespace Railgun.Events
{
    public class ConsoleLogEvent : IEvent
    {
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;

        public ConsoleLogEvent(MasterConfig config, DiscordShardedClient client)
        {
            _config = config;
            _client = client;
        }

        public void Load() => _client.Log += (message) =>
        {
            Task.Run(() => ExecuteAsync(message)).ConfigureAwait(false);
            return Task.CompletedTask;
        };

        private Task ExecuteAsync(LogMessage message)
        {
            SystemUtilities.ChangeConsoleColor(message.Severity);
            SystemUtilities.LogToConsoleAndFile(message);
            return Task.CompletedTask;
        }
    }
}