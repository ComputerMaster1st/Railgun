using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Managers;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands.Root
{
    [Alias("root"), BotAdmin]
    public partial class Root : SystemBase
    {
        private readonly TreeDiagramContext _dbContext;
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly PlayerManager _playerManager;
        private readonly TimerManager _timerManager;

        public Root(TreeDiagramContext dbContext, MasterConfig config, DiscordShardedClient client, PlayerManager playerManager, TimerManager timerManager) {
            _dbContext = dbContext;
            _config = config;
            _client = client;
            _playerManager = playerManager;
            _timerManager = timerManager;
        }
        
        [Command("show")]
        public async Task ShowAsync() {
            var masterGuild = await Context.Client.GetGuildAsync(_config.DiscordConfig.MasterGuildId);
            var masterGuildName = masterGuild != null ? masterGuild.Name : "Not Set";
            var masterGuildId = masterGuild != null ? masterGuild.Id : 0;
            
            var audiochordTc = _config.DiscordConfig.BotLogChannels.AudioChord != 0 ? 
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.AudioChord) : null;
            var commandTc = _config.DiscordConfig.BotLogChannels.CommandMngr != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.CommandMngr) : null;
            var defaultTc = _config.DiscordConfig.BotLogChannels.Common != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.Common) : null;
            var guildTc = _config.DiscordConfig.BotLogChannels.GuildMngr != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.GuildMngr) : null;
            var musicTc = _config.DiscordConfig.BotLogChannels.MusicMngr != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.MusicMngr) : null;
            var playerTc = _config.DiscordConfig.BotLogChannels.MusicPlayer != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.MusicPlayer) : null;
            var timerTc = _config.DiscordConfig.BotLogChannels.TimerMngr != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.TimerMngr) : null;
            var taskTc = _config.DiscordConfig.BotLogChannels.TaskSch != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.TaskSch) : null;
            var botlogDefault = defaultTc != null ? "Ref. BotLog-Default" : "Not Set";
            
            var botLogOutput = new StringBuilder()
                .AppendLine("TreeDiagram Logging")
                .AppendLine()
                .AppendFormat("        Default : {0}", defaultTc != null ? $"{defaultTc.Name} ({defaultTc.Id})" : "Not Set").AppendLine()
                .AppendFormat("     AudioChord : {0}", audiochordTc != null ? $"{audiochordTc.Name} ({audiochordTc.Id})" : botlogDefault).AppendLine()
                .AppendFormat("Command Manager : {0}", commandTc != null ? $"{commandTc.Name} ({commandTc.Id})" : botlogDefault).AppendLine()
                .AppendFormat("  Guild Manager : {0}", guildTc != null ? $"{guildTc.Name} ({guildTc.Id})" : botlogDefault).AppendLine()
                .AppendFormat("  Music Manager : {0}", musicTc != null ? $"{musicTc.Name} ({musicTc.Id})" : botlogDefault).AppendLine()
                .AppendFormat("   Music Player : {0}", playerTc != null ? $"{playerTc.Name} ({playerTc.Id})" : botlogDefault).AppendLine()
                .AppendFormat("  Timer Manager : {0}", timerTc != null ? $"{timerTc.Name} ({timerTc.Id})" : botlogDefault).AppendLine()
                .AppendFormat(" Task Scheduler : {0}", taskTc != null ? $"{taskTc.Name} ({taskTc.Id})" : botlogDefault).AppendLine();
                
            var masterUser = await masterGuild.GetUserAsync(_config.DiscordConfig.MasterAdminId);
            var masterName = masterUser != null ? $"{masterUser.Username}#{masterUser.DiscriminatorValue}" : "Not Set (Add ID to config then restart!)";
            var adminList = _config.DiscordConfig.OtherAdmins;
            var admins = new StringBuilder();
                
            if (adminList.Count > 0) {
                foreach (var id in adminList) {
                    var user = await masterGuild.GetUserAsync(id);

                    admins.AppendFormat("| {0}#{1} |", user.Username, user.DiscriminatorValue);
                }
            } else admins.AppendLine("None");
            
            var output = new StringBuilder()
                .AppendLine("Current Master Configuration").AppendLine()
                .AppendFormat("  Master Server : {0} ({1})", masterGuildName, masterGuildId).AppendLine()
                .AppendFormat("   Master Admin : {0}", masterName).AppendLine()
                .AppendFormat("         Prefix : {0}", _config.DiscordConfig.Prefix).AppendLine()
                .AppendFormat("   Other Admins : {0}", admins.ToString()).AppendLine()
                .AppendLine()
                .AppendLine(botLogOutput.ToString());

            await ReplyAsync(Format.Code(output.ToString()));
        }
        
        [Command("master")]
        public async Task MasterAsync() {
            await _config.AssignMasterGuildAsync(Context.Guild.Id);
            await ReplyAsync($"This server {Format.Bold(Context.Guild.Name)} has been set as master.");
        }
        
        [Command("serverlist"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task ServerListAsync() {
            var guilds = await Context.Client.GetGuildsAsync();
            var output = new StringBuilder()
                .AppendFormat("Railgun Connected Server List: ({0} Servers Listed)", guilds.Count).AppendLine().AppendLine();
            
            foreach (var guild in guilds) output.AppendFormat("{0} : {1}", guild.Id, guild.Name).AppendLine();
            
            await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "Connected Servers.txt", output.ToString(), $"({guilds.Count} Servers Listed)", false);
        }
        
        [Command("updatestatus")]
        public async Task UpdateStatusAsync() {
            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help || On {_client.Guilds.Count} Servers!", type:ActivityType.Watching);
            await ReplyAsync("Playing Status has been updated!");
        }
        
        [Command("selfinvite")]
        public async Task SelfInviteAsync(ulong id) {
            var guild = await Context.Client.GetGuildAsync(id);
            
            try {
                var invites = await guild.GetInvitesAsync();
                var output = new StringBuilder()
                    .AppendFormat("Invite for {0}", Format.Bold($"{guild.Name} ({guild.Id}")).AppendLine()
                    .AppendLine()
                    .AppendLine(invites.FirstOrDefault().Url);
                
                var masterAdmin = await Context.Client.GetUserAsync(_config.DiscordConfig.MasterAdminId);
                var dm = await masterAdmin.GetOrCreateDMChannelAsync();
                
                await dm.SendMessageAsync(output.ToString());
            } catch { await ReplyAsync($"Unable to get invites for server id {Format.Bold(id.ToString())}"); }
        }
        
        [Command("gc")]
        public Task GcAsync() {
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            return ReplyAsync("GC Forced!");
        }
        
        [Command("dc")]
        public async Task DcAsync([Remainder] string msg = null) {
            await ReplyAsync("Disconnecting ...");
            
            if (_playerManager.PlayerContainers.Count > 0) {
                var output = new StringBuilder()
                    .AppendFormat("{0} : Stopping music stream due to the following reason... {1}", 
                    Format.Bold("WARNING"), string.IsNullOrWhiteSpace(msg) ? Format.Bold("System Restart") : Format.Bold(msg));
                
                foreach (var playerInfo in _playerManager.PlayerContainers) {
                    await playerInfo.TextChannel.SendMessageAsync(output.ToString());

                    playerInfo.Player.CancelStream();
                }
            }
            
            while (_playerManager.PlayerContainers.Count > 0) await Task.Delay(1000);
            
            await _client.StopAsync();
            await _client.LogoutAsync();
        }
        
        [Command("prefix")]
        public async Task PrefixAsync([Remainder] string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                await ReplyAsync("Please specify a prefix.");

                return;
            }
            
            await _config.AssignPrefixAsync(input);
            await UpdateStatusAsync();
            await ReplyAsync($"Prefix {Format.Code($"{_config.DiscordConfig.Prefix}<command>")} is now set.");
        }
        
        [Command("eval")]
        public async Task EvalAsync([Remainder] string code) {
            var eval = new EvalUtils(_client, Context, _dbContext);
            string output;
            
            try { output = (await CSharpScript.EvaluateAsync(code, globals:eval)).ToString(); }
            catch (Exception ex) { output = ex.Message; }
            
            if (output.Length > 1900) {
                await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "evalresult.txt", output, "Evaluation Results!", false);

                return;
            }
            
            await ReplyAsync(Format.Code(output));
        }
        
        [Command("timer-restart")]
        public async Task TimerRestartAsync() {
            await _timerManager.InitializeAsync();
            await ReplyAsync("Timer Manager Restarted!");
        }
        
        [Command("avatar")]
        public async Task AvatarAsync(string url = null) {
            if (string.IsNullOrWhiteSpace(url) && Context.Message.Attachments.Count < 1) {
                await ReplyAsync("Please specify a url or upload an image.");

                return;
            }
            
            var imageUrl = !String.IsNullOrWhiteSpace(url) ? url : Context.Message.Attachments.FirstOrDefault().Url;

            using (var webclient = new HttpClient()) {
                var imageStream = await webclient.GetStreamAsync(imageUrl);

                await _client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imageStream));
                await ReplyAsync("Applied Avatar!");
            }
        }
    }
}