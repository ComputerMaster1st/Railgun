using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using Railgun.Core.Enums;

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
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.Common);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Default botlog channel.");
            }
            
            [Command("audiochord")]
            public async Task AudioAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.AudioChord);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the AudioChord botlog channel.");
            }
            
            [Command("cmdmngr")]
            public async Task CommandAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.CommandManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Command Manager botlog channel.");
            }
        
            [Command("guildmngr")]
            public async Task GuildAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.GuildManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Guild Manager botlog channel.");
            }
            
            [Command("musicmngr")]
            public async Task MusicAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.MusicManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Manager botlog channel.");
            }
            
            [Command("musicplayer-active")]
            public async Task PlayerActiveAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.MusicPlayerActive);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Player botlog channel.");
            }
            
            [Command("musicplayer-error")]
            public async Task PlayerErrorAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.MusicPlayerError);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Player botlog channel.");
            }
            
            [Command("tasksch")]
            public async Task TaskAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.TaskScheduler);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Task Scheduler botlog channel.");
            }
            
            [Command("timermngr")]
            public async Task TimerAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.TimerManager);
                await ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Timer Manager botlog channel.");
            }
        }
    }
}