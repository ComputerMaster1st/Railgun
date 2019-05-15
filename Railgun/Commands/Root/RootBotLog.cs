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
            public Task DefaultAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.Common);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Default botlog channel.");
            }
            
            [Command("audiochord")]
            public Task AudioAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.AudioChord);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the AudioChord botlog channel.");
            }
            
            [Command("cmdmngr")]
            public Task CommandAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.CommandManager);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Command Manager botlog channel.");
            }
        
            [Command("guildmngr")]
            public Task GuildAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.GuildManager);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Guild Manager botlog channel.");
            }
            
            [Command("musicmngr")]
            public Task MusicAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.MusicManager);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Manager botlog channel.");
            }
            
            [Command("musicplayer-active")]
            public Task PlayerActiveAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.MusicPlayerActive);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Player botlog channel.");
            }
            
            [Command("musicplayer-error")]
            public Task PlayerErrorAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.MusicPlayerError);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Player botlog channel.");
            }
            
            [Command("tasksch")]
            public Task TaskAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.TaskScheduler);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Task Scheduler botlog channel.");
            }
            
            [Command("timermngr")]
            public Task TimerAsync() {
                _config.AssignBotLogChannel(Context.Channel.Id, BotLogType.TimerManager);
                return ReplyAsync($"This channel {Format.Bold(Context.Channel.Name)} has been set as the Timer Manager botlog channel.");
            }
        }
    }
}