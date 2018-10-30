Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.CodeAnalysis.CSharp.Scripting
Imports RailgunVB.Core
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Preconditions
Imports RailgunVB.Core.Utilities
Imports TreeDiagram

Namespace Commands.Root
    
    <Group("root"), BotAdmin>
    Public Class Root
        Inherits SystemBase
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _client As DiscordShardedClient
        Private ReadOnly _dbContext As TreeDiagramContext
        Private ReadOnly _playerManager As PlayerManager
        Private ReadOnly _timerManager As TimerManager

        Public Sub New(config As MasterConfig, client As DiscordShardedClient, dbContext As TreeDiagramContext, 
                       playerManager As PlayerManager, timerManager As TimerManager)
            _config = config
            _client = client
            _dbContext = dbContext
            _playerManager = playerManager
            _timerManager = timerManager
        End Sub
        
        <Command("show")>
        Public Async Function ShowAsync() As Task
            Dim masterGuild As IGuild = Await Context.Client.GetGuildAsync(_config.DiscordConfig.MasterGuildId)
            Dim masterGuildName As String = If(masterGuild IsNot Nothing, masterGuild.Name, "Not Set")
            Dim masterGuildId As ULong = If(masterGuild IsNot Nothing, masterGuild.Id, 0)
            
            Dim audiochordTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.AudioChord <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.AudioChord), Nothing)
            Dim commandTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.CommandMngr <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.CommandMngr), Nothing)
            Dim defaultTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.Common <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.Common), Nothing)
            Dim guildTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.GuildMngr <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.GuildMngr), Nothing)
            Dim musicTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.MusicMngr <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.MusicMngr), Nothing)
            Dim playerTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.MusicPlayer <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.MusicPlayer), Nothing)
            Dim timerTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.TimerMngr <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.TimerMngr), Nothing)
            Dim taskTc As ITextChannel = If(_config.DiscordConfig.BotLogChannels.TaskSch <> 0,
                Await masterGuild.GetTextChannelAsync(_config.DiscordConfig.BotLogChannels.TaskSch), Nothing)
            
            Dim botLogOutput As New StringBuilder
                
            botLogOutput.AppendLine("TreeDiagram Logging") _ 
                .AppendLine() _ 
                .AppendFormat("        Default : {0}", If(defaultTc IsNot Nothing, 
                    $"{defaultTc.Name} ({defaultTc.Id})", "Not Set")).AppendLine() _ 
                .AppendFormat("     AudioChord : {0}", If(audiochordTc IsNot Nothing, 
                    $"{audiochordTc.Name} ({audiochordTc.Id})",  If(defaultTc IsNot Nothing, 
                        "Ref. BotLog-Default", "Not Set"))).AppendLine() _ 
                .AppendFormat("Command Manager : {0}", If(commandTc IsNot Nothing, 
                    $"{commandTc.Name} ({commandTc.Id})", If(defaultTc IsNot Nothing, 
                        "Ref. BotLog-Default", "Not Set"))).AppendLine() _
                .AppendFormat("  Guild Manager : {0}", If(guildTc IsNot Nothing, 
                    $"{guildTc.Name} ({guildTc.Id})", If(defaultTc IsNot Nothing, 
                        "Ref. BotLog-Default", "Not Set"))).AppendLine() _
                .AppendFormat("  Music Manager : {0}", If(musicTc IsNot Nothing, 
                    $"{musicTc.Name} ({musicTc.Id})", If(defaultTc IsNot Nothing, 
                        "Ref. BotLog-Default", "Not Set"))).AppendLine() _
                .AppendFormat("   Music Player : {0}", If(playerTc IsNot Nothing, 
                    $"{playerTc.Name} ({playerTc.Id})", If(defaultTc IsNot Nothing, 
                        "Ref. BotLog-Default", "Not Set"))).AppendLine() _
                .AppendFormat("  Timer Manager : {0}", If(timerTc IsNot Nothing, 
                     $"{timerTc.Name} ({timerTc.Id})", If(defaultTc IsNot Nothing, 
                         "Ref. BotLog-Default", "Not Set"))).AppendLine() _
                .AppendFormat(" Task Scheduler : {0}", If(taskTc IsNot Nothing, 
                    $"{taskTc.Name} ({taskTc.Id})", If(defaultTc IsNot Nothing, 
                         "Ref. BotLog-Default", "Not Set"))).AppendLine()
                
            Dim masterUser As IGuildUser = Await masterGuild.GetUserAsync(_config.DiscordConfig.MasterAdminId)
            Dim masterName As String = If(masterUser IsNot Nothing, 
                                          $"{masterUser.Username}#{masterUser.DiscriminatorValue}",
                                          "Not Set (Add ID to config then restart!)")
            Dim adminList As List(Of ULong) = _config.DiscordConfig.OtherAdmins
            Dim admins As New StringBuilder
                
            If adminList.Count > 0
                For Each id As ULong in adminList
                    Dim user As IGuildUser = Await masterGuild.GetUserAsync(id)
                    admins.AppendFormat("| {0}#{1} |", user.Username, user.DiscriminatorValue)
                Next
            Else
                admins.AppendLine("None")
            End If
            
            Dim output As New StringBuilder
            
            output.AppendLine("Current Master Configuration").AppendLine() _
                .AppendFormat("  Master Server : {0} ({1})", masterGuildName, masterGuildId).AppendLine() _
                .AppendFormat("   Master Admin : {0}", masterName).AppendLine() _
                .AppendFormat("         Prefix : {0}", _config.DiscordConfig.Prefix).AppendLine() _
                .AppendFormat("   Other Admins : {0}", admins.ToString()).AppendLine() _
                .AppendLine() _
                .AppendLine(botLogOutput.ToString())

            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
        <Command("master")>
        Public Async Function MasterAsync() As Task
            Await _config.AssignAsMasterGuildAsync(Context.Guild.Id)
            await ReplyAsync($"This server {Format.Bold(Context.Guild.Name)} has been set as master.")
        End Function
        
        <Command("serverlist"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function ServerListAsync() As Task
            Dim guilds = Await Context.Client.GetGuildsAsync()
            Dim output As New StringBuilder
            
            output.AppendFormat("Railgun Connected Server List: ({0} Servers Listed)", guilds.Count).AppendLine().AppendLine()
            
            For Each guild As IGuild in guilds
                output.AppendFormat("{0} : {1}", guild.Id, guild.Name).AppendLine()
            Next
            
            Const filename As String = "Connected Servers.txt"
            
            await File.WriteAllTextAsync(filename, output.ToString())
            await Context.Channel.SendFileAsync(filename, $"({guilds.Count} Servers Listed)")
            File.Delete(filename)
        End Function
        
        <Command("updatestatus")>
        Public Async Function UpdateStatusAsync() As Task
            await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help || On {_client.Guilds.Count} Servers!", 
                                       type := ActivityType.Watching)
            await ReplyAsync("Playing Status has been updated!")
        End Function
        
        <Command("selfinvite")>
        Public Async Function SelfInviteAsync(id As ULong) As Task
            Dim guild As IGuild = Await Context.Client.GetGuildAsync(id)
            
            Try
                Dim invites = Await guild.GetInvitesAsync()
                Dim output As New StringBuilder
                
                output.AppendFormat("Invite for {0}", Format.Bold($"{guild.Name} ({guild.Id}")).AppendLine() _
                    .AppendLine() _
                    .AppendLine(invites.FirstOrDefault().Url)
                
                Dim masterAdmin As IUser = Await Context.Client.GetUserAsync(_config.DiscordConfig.MasterAdminId)
                Dim dm As IDMChannel = Await masterAdmin.GetOrCreateDMChannelAsync()
                
                Await dm.SendMessageAsync(output.ToString())
            Catch
                ReplyAsync($"Unable to get invites for server id {Format.Bold(id.ToString())}").GetAwaiter()
            End Try
        End Function
        
        <Command("gc")>
        Public Async Function GcAsync() As Task
            Gc.WaitForPendingFinalizers()
            Gc.Collect()
            
            await ReplyAsync("GC Forced!")
        End Function
        
        <Command("dc")>
        Public Async Function DcAsync(<Remainder> Optional msg As String = Nothing) As Task
            await ReplyAsync("Disconnecting ...")
            
            If _playerManager.PlayerContainers.Count > 0
                Dim output As New StringBuilder
                
                output.AppendFormat("{0} : Stopping music stream due to the following reason... {1}", 
                    Format.Bold("WARNING"), If(string.IsNullOrWhiteSpace(msg), Format.Bold("System Restart"), Format.Bold(msg)))
                
                For Each playerInfo In _playerManager.PlayerContainers
                    Await playerInfo.TextChannel.SendMessageAsync(output.ToString())
                    playerInfo.Player.CancelStream()
                Next
            End If
            
            While _playerManager.PlayerContainers.Count > 0
                Await Task.Delay(1000)
            End While
            
            Await _client.StopAsync()
            Await _client.LogoutAsync()
        End Function
        
        <Command("prefix")>
        Public Async Function PrefixAsync(<Remainder> input As String) As Task
            If String.IsNullOrWhiteSpace(input)
                await ReplyAsync("Please specify a prefix.")
                Return
            End If
            
            Await _config.AssignPrefixAsync(input)
            Await UpdateStatusAsync()
            await ReplyAsync($"Prefix {Format.Code($"{_config.DiscordConfig.Prefix}<command>")} is now set.")
        End Function
        
        <Command("eval")>
        Public Async Function EvalAsync(<Remainder> code As String) As Task
            Dim eval As New EvalUtils(_client, Context, _dbContext)
            Dim output As String
            
            Try
                output = (Await CSharpScript.EvaluateAsync(code, globals := eval)).ToString()
            Catch ex As Exception
                output = ex.Message
            End Try
            
            If output.Length > 1900
                Const filename = "evalresult.txt"
                
                await File.WriteAllTextAsync(filename, output)
                await Context.Channel.SendFileAsync(filename, "Evaluation Results!")
                File.Delete(filename)
                return
            End If
            
            await ReplyAsync(Format.Code(output))
        End Function
        
        <Command("timer-restart")>
        Public Async Function TimerRestartAsync() As Task
            await _timerManager.Initialize()
            await ReplyAsync("Timer Manager Restarted!")
        End Function
        
        <Command("avatar")>
        Public Async Function AvatarAsync(Optional url As String = Nothing) As Task
            If String.IsNullOrWhiteSpace(url) AndAlso Context.Message.Attachments.Count < 1
                Await ReplyAsync("Please specify a url or upload an image.")
                Return
            End If
            
            Dim imageUrl As String = If(Not (String.IsNullOrWhiteSpace(url)), url, 
                                        Context.Message.Attachments.FirstOrDefault.Url)
            Dim webclient As New HttpClient
            Dim imageStream As Stream = Await webclient.GetStreamAsync(imageUrl)
            
            Await _client.CurrentUser.ModifyAsync(Sub(x) x.Avatar = New Image(imageStream))
            Await ReplyAsync("Applied Avatar!")
            
            webclient.Dispose()
        End Function
        
    End Class
    
End NameSpace