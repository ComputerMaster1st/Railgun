using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Extensions;
using Railgun.Music;
using Railgun.Timers;
using Railgun.Utilities;

namespace Railgun.Commands.Root
{
    [Alias("root"), BotAdmin]
    public partial class Root : SystemBase
    {
        private readonly MasterConfig _config;
        private readonly DiscordShardedClient _client;
        private readonly PlayerController _playerController;
        private readonly TimerController _timerManager;

        public Root(MasterConfig config, DiscordShardedClient client, PlayerController playerController, TimerController timerController) {
            _config = config;
            _client = client;
            _playerController = playerController;
            _timerManager = timerController;
        }
        
        [Command("show")]
        public async Task ShowAsync() {
            var masterGuild = await Context.Client.GetGuildAsync(_config.DiscordConfig.MasterGuildId);
            var masterGuildName = masterGuild != null ? masterGuild.Name : "Not Set";
            var masterGuildId = masterGuild?.Id ?? 0;
            
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
            var playerActiveTc = _config.DiscordConfig.BotLogChannels.MusicPlayerActive != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.MusicPlayerActive) : null;
            var playerErrorTc = _config.DiscordConfig.BotLogChannels.MusicPlayerError != 0 ?
                await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.MusicPlayerError) : null;
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
                .AppendFormat("   Music Player : {0}", playerActiveTc != null ? $"{playerActiveTc.Name} ({playerActiveTc.Id})" : botlogDefault).AppendLine()
                .AppendFormat("   Music Player : {0}", playerErrorTc != null ? $"{playerErrorTc.Name} ({playerErrorTc.Id})" : botlogDefault).AppendLine()
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
            _config.AssignMasterGuild(Context.Guild.Id);
            await ReplyAsync($"This server {Format.Bold(Context.Guild.Name)} has been set as master.");
        }
        
        [Command("serverlist"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task ServerListAsync() {
            var guilds = await Context.Client.GetGuildsAsync();
            var output = new StringBuilder()
                .AppendFormat("Railgun Connected Server List: ({0} Servers Listed)", guilds.Count).AppendLine().AppendLine();
            
            foreach (var guild in guilds) output.AppendFormat("{0} : {1}", guild.Id, guild.Name).AppendLine();
            
            await ((ITextChannel)Context.Channel).SendStringAsFileAsync("Connected Servers.txt", output.ToString(), $"({guilds.Count} Servers Listed)", false);
        }
        
        [Command("updatestatus")]
        public async Task UpdateStatusAsync() {
            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {SystemUtilities.GetSeparator} On {_client.Guilds.Count} Servers!", type:ActivityType.Watching);
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
            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await _client.SetGameAsync("Shutting Down ...");
            await ReplyAsync("Disconnecting ...");
            
            if (_playerController.PlayerContainers.Count > 0) {
                var output = new StringBuilder()
                    .AppendFormat("{0} : Stopping music stream due to the following reason... {1}", 
                    Format.Bold("WARNING"), string.IsNullOrWhiteSpace(msg) ? Format.Bold("System Restart") : Format.Bold(msg));
                
                foreach (var playerInfo in _playerController.PlayerContainers) {
                    await playerInfo.TextChannel.SendMessageAsync(output.ToString());

                    playerInfo.Player.CancelStream();
                }
            }
            
            while (_playerController.PlayerContainers.Count > 0) await Task.Delay(1000);
            
            await _client.StopAsync();
            await _client.LogoutAsync();
        }
        
        [Command("prefix")]
        public async Task PrefixAsync([Remainder] string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                await ReplyAsync("Please specify a prefix.");

                return;
            }
            
            _config.AssignPrefix(input);
            await UpdateStatusAsync();
            await ReplyAsync($"Prefix {Format.Code($"{_config.DiscordConfig.Prefix}<command>")} is now set.");
        }
        
        [Command("eval")]
        public async Task EvalAsync([Remainder] string code) {
            var eval = new EvalUtils(_client, Context, Context.Database);
            string output;
            
            try { output = (await CSharpScript.EvaluateAsync(code, globals:eval)).ToString(); }
            catch (Exception ex) { output = ex.Message; }
            
            if (output.Length > 1900) {
                await ((ITextChannel)Context.Channel).SendStringAsFileAsync("evalresult.txt", output, "Evaluation Results!", false);

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
            
            var imageUrl = !string.IsNullOrWhiteSpace(url) ? url : Context.Message.Attachments.FirstOrDefault().Url;

            using (var webclient = new HttpClient()) {
                var imageStream = await webclient.GetStreamAsync(imageUrl);

                await _client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imageStream));
                await ReplyAsync("Applied Avatar!");
            }
        }

        [Command("whosfrom")]
        public async Task WhosFrom(ulong serverId) {
            var guild = await Context.Client.GetGuildAsync(serverId);

            if (guild == null) {
                await ReplyAsync("Can not find specified guild/server.");
                return;
            }

            var localUsers = await Context.Guild.GetUsersAsync();
            var remoteUsers = new List<IGuildUser>();

            foreach (var localUser in localUsers) {
                var remoteUser = await guild.GetUserAsync(localUser.Id);

                if (remoteUser != null) remoteUsers.Add(remoteUser);
            }

            if (remoteUsers.Count < 1) {
                await ReplyAsync($"Unable to find users who are from {Format.Bold($"{guild.Name} <{guild.Id}>")}.");
                return;
            }

            var output = new StringBuilder()
                .AppendFormat("There are {0} from {1}!", 
                    Format.Bold($"{remoteUsers.Count} user(s)"), 
                    Format.Bold($"{guild.Name} <{guild.Id}>")).AppendLine()
                .AppendLine();

            foreach (var remoteUser in remoteUsers) {
                output.AppendFormat("{0}#{1} {2} ", remoteUser.Username, remoteUser.DiscriminatorValue, SystemUtilities.GetSeparator);
            }

            output.Remove(output.Length - 3, 3);

            await ReplyAsync(output.ToString());
        }
    }
}