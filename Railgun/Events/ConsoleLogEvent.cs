using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Events
{
    [PreInitialize]
    public class ConsoleLogEvent
    {
        public ConsoleLogEvent(DiscordShardedClient client)
            => client.Log += (message) => Task.Factory.StartNew(async () => await ExecuteAsync(message));

        private Task ExecuteAsync(LogMessage message)
        {
            SystemUtilities.ChangeConsoleColor(message.Severity);
            SystemUtilities.LogToConsoleAndFile(message);

            return Task.CompletedTask;
        }
    }
}