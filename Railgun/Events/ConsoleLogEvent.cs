using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Core;
using Railgun.Core.Configuration;

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

        public void Load() => _client.Log += (message) => Task.Factory.StartNew(() => ExecuteAsync(message));

        private Task ExecuteAsync(LogMessage message)
        {
            switch (message.Severity) {
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
            }

            SystemUtilities.LogToConsoleAndFile(message);
            return Task.CompletedTask;
        }
    }
}