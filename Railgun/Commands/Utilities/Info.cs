using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioChord;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Microsoft.EntityFrameworkCore;
using Railgun.Core.Commands;
using Railgun.Core.Configuration;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands.Utilities
{
    [Alias("info")]
    public class Info : SystemBase
    {
        private readonly TreeDiagramContext _db;
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly CommandService<SystemContext> _commandService;
        private readonly MusicService _musicService;
        private readonly PlayerManager _playerManager;
        private readonly Analytics _analytics;

        public Info(TreeDiagramContext db, MasterConfig config, DiscordShardedClient client, CommandService<SystemContext> commandService, MusicService musicService, PlayerManager playerManager, Analytics analytics) {
            _db = db;
            _config = config;
            _client = client;
            _commandService = commandService;
            _musicService = musicService;
            _playerManager = playerManager;
            _analytics = analytics;
        }

        [Command("status")]
        public async Task StatusAsync() {
            var channelCount = 0.00;
            var userCount = 0.00;
            var commandsExecuted = 0;
            var installedCommands = _commandService.GetAllCommands().Count();
            var self = Process.GetCurrentProcess();
            var guilds = await Context.Client.GetGuildsAsync();

            foreach (var guild in guilds) {
                channelCount += (await guild.GetChannelsAsync()).Count();
                userCount += (await guild.GetUsersAsync()).Count();
            }

            foreach (var command in _analytics.UsedCommands) commandsExecuted += command.Value;

            var gcOutput = new StringBuilder();

            for (var i = 0; i <= GC.MaxGeneration; i++) 
                gcOutput.AppendFormat("Gen {0} : {1} || ", i, GC.CollectionCount(i));
            
            gcOutput.Remove(gcOutput.Length - 3, 3);

            var output = new StringBuilder()
                .AppendLine("Railgun System Status")
                .AppendLine()
                .AppendFormat("Messages Intercepted : {0} ({1}/sec)", _analytics.ReceivedMessages, Math.Round(_analytics.ReceivedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                .AppendFormat("    Messages Updated : {0} ({1}/sec)", _analytics.UpdatedMessages, Math.Round(_analytics.UpdatedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                .AppendFormat("  Messages Destroyed : {0} ({1}/sec)", _analytics.DeletedMessages, Math.Round(_analytics.DeletedMessages / DateTime.Now.Subtract(self.StartTime).TotalSeconds, 4)).AppendLine()
                .AppendFormat("  Commands Available : {0}", installedCommands).AppendLine()
                .AppendFormat("   Commands Executed : {0}", commandsExecuted).AppendLine()
                .AppendFormat("       Music Streams : {0}", _playerManager.PlayerContainers.Count).AppendLine()
                .AppendLine()
                .AppendFormat("      Client Latency : {0}ms", _client.Latency).AppendLine()
                .AppendFormat("    Connected Shards : {0}", _client.Shards.Count).AppendLine()
                .AppendFormat("   Connected Servers : {0}", guilds.Count).AppendLine()
                .AppendFormat("      Total Channels : {0}", channelCount).AppendLine()
                .AppendFormat("         Total Users : {0}", userCount).AppendLine()
                .AppendFormat("    Music Repository : {0} ({1} GB)", (await _musicService.GetAllSongsAsync()).Count(), Math.Round((((await _musicService.GetTotalBytesUsedAsync()) / 1024) / 1024) / 1024, 2)).AppendLine()
                .AppendLine()
                .AppendFormat("       Avg. Channels : {0}/server", Math.Round(channelCount / guilds.Count, 0)).AppendLine()
                .AppendFormat("          Avg. Users : {0}/server", Math.Round(userCount / guilds.Count, 0)).AppendLine()
                .AppendLine()
                .AppendFormat("          Started At : {0}", self.StartTime).AppendLine()
                .AppendFormat("              Uptime : {0}", DateTime.Now - self.StartTime).AppendLine()
                .AppendLine()
                .AppendFormat("            CPU Time : {0}", self.TotalProcessorTime).AppendLine()
                .AppendFormat("             Threads : {0}", self.Threads.Count).AppendLine()
                .AppendFormat("     Physical Memory : {0} MB", Math.Round((self.WorkingSet64 / 1024.0) / 1024, 2)).AppendLine()
                .AppendFormat("        Paged Memory : {0} MB", Math.Round((self.PagedMemorySize64 / 1024.0) / 1024, 2)).AppendLine()
                .AppendLine()
                .AppendFormat("  Garbage Collection : {0}", gcOutput.ToString()).AppendLine()
                .AppendLine()
                .AppendLine("End of Report!");
            
            await ReplyAsync(Format.Code(output.ToString()));
        }

        [Command("config")]
        public async Task ConfigAsync() {
            var output = new StringBuilder();

            output.AppendLine("TreeDiagram Configuration Report")
                .AppendLine().AppendLine("Server/Guild Configurations :")
                .AppendFormat("Anti-Caps : {0}", await _db.FilterCapses.CountAsync()).AppendLine()
                .AppendFormat(" Anti-Url : {0}", await _db.FilterUrls.CountAsync()).AppendLine()
                .AppendFormat("     Bite : {0}", await _db.FunBites.CountAsync()).AppendLine()
                .AppendFormat("      RST : {0}", await _db.FunRsts.CountAsync()).AppendLine()
                .AppendFormat("  Command : {0}", await _db.ServerCommands.CountAsync())
                .AppendFormat("JoinLeave : {0}", await _db.ServerJoinLeaves.CountAsync()).AppendLine()
                .AppendFormat("  Mention : {0}", await _db.ServerMentions.CountAsync()).AppendLine()
                .AppendFormat("    Music : {0}", await _db.ServerMusics.CountAsync()).AppendLine()
                .AppendFormat("  Warning : {0}", await _db.ServerWarnings.CountAsync()).AppendLine()
                .AppendLine().AppendLine("User Configurations :")
                .AppendFormat("  Mention : {0}", await _db.UserMentions.CountAsync()).AppendLine()
                .AppendFormat("  Command : {0}", await _db.UserCommands.CountAsync()).AppendLine()
                .AppendLine()
                .AppendLine("End of Report!");
            
            await ReplyAsync(Format.Code(output.ToString()));
        }

        [Command("dev")]
        public async Task DeveloperAsync() {
            var output = new StringBuilder()
                .AppendFormat("Railgun has been written by {0}.", Format.Bold("ComputerMaster1st#6458")).AppendLine()
                .AppendFormat("If you have any problems, issues, suggestions, etc, {0} can be found on this discord: {1}", Format.Bold("ComputerMaster1st"), Format.Bold("<https://discord.gg/Czw5ffx>")).AppendLine();

            await ReplyAsync(output.ToString());
        }

        [Command("commands")]
        public async Task CommandAnalyticsAsync() {
            var commands = _analytics.UsedCommands.OrderByDescending(r => r.Value);
            var count = 20;
            var output = new StringBuilder()
                .AppendFormat("Railgun Top {0} Command Analytics:", count).AppendLine().AppendLine();
            
            foreach (var command in commands) {
                output.AppendFormat("{0} <= {1}", command.Value, command.Key).AppendLine();
                count--;

                if (count < 1) break;
            }

            await ReplyAsync(Format.Code(output.ToString()));
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
        public async Task TimersAsync() {
            var output = new StringBuilder()
                .AppendLine("TreeDiagram Timers Status")
                .AppendLine()
                .AppendFormat("Remind Me : {0}", _db.TimerRemindMes.CountAsync());
            
            await ReplyAsync(Format.Code(output.ToString()));
        }
    }
}