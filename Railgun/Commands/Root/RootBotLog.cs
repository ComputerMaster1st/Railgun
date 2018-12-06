using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Configuration;
using Railgun.Core.Logging;

namespace Railgun.Commands.Root
{
    public partial class Root 
    {
        [Alias("botlog")]
        public class RootBotLog : SystemBase
        {
            private readonly MasterConfig _config;

            public RootBotLog(MasterConfig config) => _config = config;
        
            [Command]
            public async Task DefaultAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.Common);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Default botlog channel.");
            }
            
            [Command("audiochord")]
            public async Task AudioAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.AudioChord);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the AudioChord botlog channel.");
            }
            
            [Command("cmdmngr")]
            public async Task CommandAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.CommandManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Command Manager botlog channel.");
            }
        
            [Command("guildmngr")]
            public async Task GuildAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.GuildManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Guild Manager botlog channel.");
            }
            
            [Command("musicmngr")]
            public async Task MusicAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.MusicManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Manager botlog channel.");
            }
            
            [Command("musicplayer")]
            public async Task PlayerAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.MusicPlayer);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Player botlog channel.");
            }
            
            [Command("tasksch")]
            public async Task TaskAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.TaskScheduler);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Task Scheduler botlog channel.");
            }
            
            [Command("timermngr")]
            public async Task TimerAsync() {
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.TimerManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Timer Manager botlog channel.");
            }
        }
    }
}