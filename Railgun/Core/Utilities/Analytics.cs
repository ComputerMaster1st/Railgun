using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Results;
using Railgun.Core.Logging;

namespace Railgun.Core.Utilities
{
    public class Analytics
    {
        private readonly DiscordShardedClient _client;
        private readonly Log _log;

        public uint ReceivedMessages { get; private set; } = 0;
        public uint UpdatedMessages { get; private set; } = 0;
        public uint DeletedMessages { get; set; } = 0;

        public Dictionary<string, int> UsedCommands { get; } = new Dictionary<string, int>();

        public Analytics(Log log, DiscordShardedClient client) {
            _log = log;
            _client = client;

            _client.MessageReceived += MessageReceivedAsync;
            _client.MessageUpdated += MessageUpdatedAsync;
            _client.MessageDeleted += MessageDeletedAsync;
        }

        private Task MessageReceivedAsync(SocketMessage message) {
            ReceivedMessages++;

            return Task.CompletedTask;
        }

        private Task MessageUpdatedAsync(Cacheable<IMessage, ulong> oldMsg, SocketMessage newMsg, ISocketMessageChannel channel) {
            UpdatedMessages++;

            return Task.CompletedTask;
        }

        private Task MessageDeletedAsync(Cacheable<IMessage, ulong> oldMsg, ISocketMessageChannel channel) {
            DeletedMessages++;

            return Task.CompletedTask;
        }

        public async Task ExecutedCommandAsync(CommandResult result) {
            var cmdString = result.CommandPath;
            var ctx = result.Context;
            var guild = ctx.Guild;

            if (cmdString != "N/A") {
                if (UsedCommands.ContainsKey(cmdString)) UsedCommands[cmdString]++;
                else UsedCommands.Add(cmdString, 1);
            }

            var output = new StringBuilder()
                .AppendFormat("<{0} <{1}>>", guild.Name, guild.Id).AppendLine()
                .AppendFormat("---- Command : {0}", cmdString).AppendLine();
            
            if (ctx.Message.Content.Length < 250)
                output.AppendFormat("---- Content : {0}", ctx.Message.Content).AppendLine();

            output.AppendLine("---- Result  : Completed");

            await _log.LogToBotLogAsync(output.ToString(), BotLogType.CommandManager);
        }
    }
}