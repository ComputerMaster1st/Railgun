Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Logging

Namespace Commands.Root

    Partial Public Class Root
    
        <Group("botlog")>
        Public Class RootBotlog
            Inherits  ModuleBase
        
            Private ReadOnly _config As MasterConfig

            Public Sub New(config As MasterConfig)
                _config = config
            End Sub
        
            <Command>
            Public Async Function DefaultAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.Common)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the Default botlog channel.")
            End Function
            
            <Command("audiochord")>
            Public Async Function AudioAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.AudioChord)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the AudioChord botlog channel.")
            End Function
            
            <Command("cmdmngr")>
            Public Async Function CommandAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.CommandManager)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the Command Manager botlog channel.")
            End Function
        
            <Command("guildmngr")>
            Public Async Function GuildAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.GuildManager)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the Guild Manager botlog channel.")
            End Function
            
            <Command("musicmngr")>
            Public Async Function MusicAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.MusicManager)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Manager botlog channel.")
            End Function
            
            <Command("musicplayer")>
            Public Async Function PlayerAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.MusicPlayer)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the Music Player botlog channel.")
            End Function
            
            <Command("tasksch")>
            Public Async Function TaskAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.TaskScheduler)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the Task Scheduler botlog channel.")
            End Function
            
            <Command("timermngr")>
            Public Async Function TimerAsync() As Task
                await _config.AssignBotLogChannelAsync(Context.Channel.Id, BotLogType.TimerManager)
                await ReplyAsync(
                    $"This channel {Format.Bold(Context.Channel.Name)} has been set as the Timer Manager botlog channel.")
            End Function
            
        End Class
    
    End Class
    
End NameSpace