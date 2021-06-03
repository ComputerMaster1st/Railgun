using AudioChord;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using Railgun.Music;
using Railgun.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Railgun.Commands.Info
{
    public partial class Info
    {
        [Alias("status")]
        public class InfoStatus : SystemBase
        {
            private readonly DiscordShardedClient _client;
            private readonly CommandService<SystemContext> _commandService;
            private readonly MusicService _musicService;
            private readonly Analytics _analytics;
            private readonly PlayerController _players;

            public InfoStatus(DiscordShardedClient client, CommandService<SystemContext> commandService, MusicService musicService, Analytics analytics, PlayerController players)
            {
                _client = client;
                _commandService = commandService;
                _musicService = musicService;
                _analytics = analytics;
                _players = players;
            }

            [Command]
            public async Task ExecuteAsync()
            {
                var commandsExecuted = 0;
                var installedCommands = _commandService
                    .GetAllCommands()
                    .Count();
                var self = Process.GetCurrentProcess();
                var guilds = await Context.Client.GetGuildsAsync();
                var directorySize = new DirectoryInfo("/home/audiochord")
                    .EnumerateFiles()
                    .Sum(file => file.Length);

                foreach (var command in _analytics.UsedCommands)
                    commandsExecuted += command.Value;

                var gcOutput = new StringBuilder();

                for (var i = 0; i <= GC.MaxGeneration; i++)
                    gcOutput.AppendFormat("Gen {0} : {1} {2} ",
                        i,
                        GC.CollectionCount(i),
                        SystemUtilities.GetSeparator
                    );

                gcOutput.Remove(gcOutput.Length - 2, 2);

                var output = new StringBuilder()
                    .AppendLine("Railgun System Status")
                    .AppendLine()
                    .AppendFormat(" Messages Received : {0} ({1}/sec)", 
                        _analytics.ReceivedMessages, 
                        Math.Round(_analytics.ReceivedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                    .AppendFormat("  Messages Updated : {0} ({1}/sec)",
                        _analytics.UpdatedMessages, 
                        Math.Round(_analytics.UpdatedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                    .AppendFormat("  Messages Deleted : {0} <{1}> ({2}/sec)", 
                        _analytics.DeletedMessages,
                        _analytics.FilterDeletedMessages, 
                        Math.Round(_analytics.DeletedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                    .AppendFormat("Commands Available : {0}", installedCommands).AppendLine()
                    .AppendFormat(" Commands Executed : {0}", commandsExecuted).AppendLine()
                    .AppendLine()
                    .AppendFormat("    Client Latency : {0}ms", _client.Latency).AppendLine()
                    .AppendFormat("  Connected Shards : {0}", _client.Shards.Count).AppendLine()
                    .AppendFormat(" Connected Servers : {0}", guilds.Count).AppendLine()
                    .AppendLine()
                    .AppendFormat("     Music Streams : {0}", _players.PlayerContainers.Count).AppendLine()
                    .AppendFormat("  Music Repository : {0} ({1} GB)", 
                        await _musicService.EnumerateSongMetadataAsync()
                            .CountAsync(), 
                        Math.Round(Convert.ToDecimal(directorySize) / 1024 / 1024 / 1024, 2)).AppendLine()
                    .AppendLine()
                    .AppendFormat("        Started At : {0}", self.StartTime).AppendLine()
                    .AppendFormat("            Uptime : {0}", DateTime.Now - self.StartTime).AppendLine()
                    .AppendLine()
                    .AppendFormat("          CPU Time : {0}", self.TotalProcessorTime).AppendLine()
                    .AppendFormat("           Threads : {0}", self.Threads.Count).AppendLine()
                    .AppendFormat("   Physical Memory : {0} MB", Math.Round(self.WorkingSet64 / 1024.0 / 1024, 2)).AppendLine()
                    .AppendFormat("      Paged Memory : {0} MB", Math.Round(self.PagedMemorySize64 / 1024.0 / 1024, 2)).AppendLine()
                    .AppendLine()
                    .AppendFormat("Garbage Collection : {0}", gcOutput.ToString()).AppendLine()
                    .AppendLine()
                    .AppendLine("End of Report!");

                self.Dispose();

                await ReplyAsync(Format.Code(output.ToString()));
            }
        }
    }
}
