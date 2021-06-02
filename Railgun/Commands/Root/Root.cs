using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public Task MasterAsync() {
            _config.AssignMasterGuild(Context.Guild.Id);
            return ReplyAsync($"This server {Format.Bold(Context.Guild.Name)} has been set as master.");
        }
        
        [Command("serverlist"), BotPerms(ChannelPermission.AttachFiles)]
        public async Task ServerListAsync() {
            var guilds = await Context.Client.GetGuildsAsync();
            var output = new StringBuilder()
                .AppendFormat("Railgun Connected Server List: ({0} Servers Listed)", guilds.Count).AppendLine().AppendLine();
            
            foreach (var guild in guilds) output.AppendFormat("{0} : {1}", guild.Id, guild.Name).AppendLine();
            await (Context.Channel as ITextChannel).SendStringAsFileAsync("Connected Servers.txt", output.ToString(), $"({guilds.Count} Servers Listed)", false);
        }
        
        [Command("updatestatus")]
        public async Task UpdateStatusAsync() {
            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help {SystemUtilities.GetSeparator} On {_client.Guilds.Count} Servers!", type:ActivityType.Watching);
            await ReplyAsync("Playing Status has been updated!");
        }
    }
}