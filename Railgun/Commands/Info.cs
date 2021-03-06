using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Music;
using Railgun.Utilities;

namespace Railgun.Commands
{
    [Alias("info")]
    public class Info : SystemBase
    {
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly CommandService<SystemContext> _commandService;
        private readonly MusicService _musicService;
        private readonly PlayerController _playerController;
        private readonly Analytics _analytics;

        public Info(MasterConfig config, DiscordShardedClient client, CommandService<SystemContext> commandService, MusicService musicService, PlayerController playerController, Analytics analytics) {
            _config = config;
            _client = client;
            _commandService = commandService;
            _musicService = musicService;
            _playerController = playerController;
            _analytics = analytics;
        }

        [Command("status")]
        public async Task StatusAsync() {
            var commandsExecuted = 0;
            var installedCommands = _commandService.GetAllCommands().Count();
            var self = Process.GetCurrentProcess();
            var guilds = await Context.Client.GetGuildsAsync();
            var directorySize = new DirectoryInfo("/home/audiochord").EnumerateFiles().Sum(file => file.Length);

            foreach (var command in _analytics.UsedCommands) commandsExecuted += command.Value;

            var gcOutput = new StringBuilder();

            for (var i = 0; i <= GC.MaxGeneration; i++) 
                gcOutput.AppendFormat("Gen {0} : {1} {2} ", i, GC.CollectionCount(i), SystemUtilities.GetSeparator);
            
            gcOutput.Remove(gcOutput.Length - 2, 2);

            var output = new StringBuilder()
                .AppendLine("Railgun System Status")
                .AppendLine()
                .AppendFormat(" Messages Received : {0} ({1}/sec)", _analytics.ReceivedMessages, Math.Round(_analytics.ReceivedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                .AppendFormat("  Messages Updated : {0} ({1}/sec)", _analytics.UpdatedMessages, Math.Round(_analytics.UpdatedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                .AppendFormat("  Messages Deleted : {0} <{1}> ({2}/sec)", _analytics.DeletedMessages, _analytics.FilterDeletedMessages, Math.Round(_analytics.DeletedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                .AppendFormat("Commands Available : {0}", installedCommands).AppendLine()
                .AppendFormat(" Commands Executed : {0}", commandsExecuted).AppendLine()
                .AppendLine()
                .AppendFormat("    Client Latency : {0}ms", _client.Latency).AppendLine()
                .AppendFormat("  Connected Shards : {0}", _client.Shards.Count).AppendLine()
                .AppendFormat(" Connected Servers : {0}", guilds.Count).AppendLine()
                .AppendLine()
                .AppendFormat("     Music Streams : {0}", _playerController.PlayerContainers.Count).AppendLine()
                .AppendFormat("  Music Repository : {0} ({1} GB)", await _musicService.EnumerateSongMetadataAsync().CountAsync(), Math.Round((((Convert.ToDecimal(directorySize)) / 1024) / 1024) / 1024, 2)).AppendLine()
                .AppendLine()
                .AppendFormat("        Started At : {0}", self.StartTime).AppendLine()
                .AppendFormat("            Uptime : {0}", DateTime.Now - self.StartTime).AppendLine()
                .AppendLine()
                .AppendFormat("          CPU Time : {0}", self.TotalProcessorTime).AppendLine()
                .AppendFormat("           Threads : {0}", self.Threads.Count).AppendLine()
                .AppendFormat("   Physical Memory : {0} MB", Math.Round((self.WorkingSet64 / 1024.0) / 1024, 2)).AppendLine()
                .AppendFormat("      Paged Memory : {0} MB", Math.Round((self.PagedMemorySize64 / 1024.0) / 1024, 2)).AppendLine()
                .AppendLine()
                .AppendFormat("Garbage Collection : {0}", gcOutput.ToString()).AppendLine()
                .AppendLine()
                .AppendLine("End of Report!");
            
            self.Dispose();
            await ReplyAsync(Format.Code(output.ToString()));
        }

        [Command("config", "configs")]
        public Task ConfigAsync() {
            var output = new StringBuilder();

            output.AppendLine("TreeDiagram Configuration Report")
                .AppendLine()
                .AppendFormat("  Servers/Guilds : {0}", Context.Database.ServerProfiles.Count()).AppendLine()
                .AppendFormat("           Users : {0}", Context.Database.UserProfiles.Count()).AppendLine()
                .AppendLine()
                .AppendFormat("Assign Role Timers : {0}", Context.Database.TimerAssignRoles.Count()).AppendLine()
                .AppendFormat("  Kick User Timers : {0}", Context.Database.TimerKickUsers.Count()).AppendLine()
                .AppendFormat("  Remind Me Timers : {0}", Context.Database.TimerRemindMes.Count()).AppendLine()
                .AppendLine()
                .AppendLine("End of Report!");
            
            return ReplyAsync(Format.Code(output.ToString()));
        }

        [Command("dev")]
        public Task DeveloperAsync() {
            var output = new StringBuilder()
                .AppendFormat("Railgun has been written by {0}.", Format.Bold("ComputerMaster1st#6458")).AppendLine()
                .AppendFormat("If you have any problems, issues, suggestions, etc, {0} can be found on this discord: {1}", Format.Bold("ComputerMaster1st"), Format.Bold("<https://discord.gg/Czw5ffx>")).AppendLine();
            return ReplyAsync(output.ToString());
        }

        [Command("commands")]
        public Task CommandAnalyticsAsync() {
            var commands = _analytics.UsedCommands.OrderByDescending(r => r.Value);
            var count = 20;
            var output = new StringBuilder()
                .AppendFormat("Railgun Top {0} Command Analytics:", count).AppendLine().AppendLine();
            
            foreach (var command in commands) {
                output.AppendFormat("{0} <= {1}", command.Value, command.Key).AppendLine();
                count--;

                if (count < 1) break;
            }

            return ReplyAsync(Format.Code(output.ToString()));
        }

        [Command("admins")]
        public async Task AdminsAsync() {
            var guild = await Context.Client.GetGuildAsync(_config.DiscordConfig.MasterGuildId);
            var master = await guild.GetUserAsync(_config.DiscordConfig.MasterAdminId);
            var output = new StringBuilder()
                .AppendFormat(Format.Bold("{0}#{1}") + " is Railgun's Master Admin.", master.Username, master.DiscriminatorValue).AppendLine();
            
            foreach (var adminId in _config.DiscordConfig.OtherAdmins) {
                var admin = await guild.GetUserAsync(adminId);

                if (admin == null) {
                    output.AppendFormat(Format.Bold("{0}"), adminId).AppendLine();
                    break;
                }

                output.AppendFormat(Format.Bold("{0}#{1}"), admin.Username, admin.DiscriminatorValue).AppendLine();
            }

            await ReplyAsync(output.ToString());
        }

        [Command("timers")]
        public Task TimersAsync() {
            var output = new StringBuilder()
                .AppendLine("TreeDiagram Timers Status")
                .AppendLine()
                .AppendFormat("Remind Me   : {0}", Context.Database.TimerRemindMes.Count()).AppendLine()
                .AppendFormat("Assign Role : {0}", Context.Database.TimerAssignRoles.Count()).AppendLine()
                .AppendFormat("Kick User   : {0}", Context.Database.TimerKickUsers.Count());
            
            return ReplyAsync(Format.Code(output.ToString()));
        }

        [Command("shards")]
        public Task ShardsAsync()
        {
            var output = new StringBuilder()
                .AppendLine("Shard Status")
                .AppendLine();

            foreach (var shard in _client.Shards)
                output.AppendFormat("SHARD #{0} => Connection State: {1}, Latency: {2}, Servers: {3}", shard.ShardId, shard.ConnectionState, shard.Latency, shard.Guilds.Count)
                    .AppendLine();

            return ReplyAsync(Format.Code(output.ToString()));
        }
    }
}