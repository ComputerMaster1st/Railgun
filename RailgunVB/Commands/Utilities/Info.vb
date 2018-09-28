Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.EntityFrameworkCore
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Utilities
Imports TreeDiagram

Namespace Commands.Utilities
    
    <Group("info")>
    Public Class Info
        Inherits ModuleBase
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _client As DiscordShardedClient
        Private ReadOnly _commandService As CommandService
        Private ReadOnly _dbContext As TreeDiagramContext
        Private ReadOnly _musicService As MusicService
        
        Private ReadOnly _analytics As Analytics
        Private ReadOnly _playerManager As PlayerManager

        Public Sub New(config As MasterConfig, client As DiscordShardedClient, commandService As CommandService, dbContext As TreeDiagramContext, musicService As MusicService, analytics As Analytics, playerManager As PlayerManager)
            _config = config
            _client = client
            _commandService = commandService
            _dbContext = dbContext
            _musicService = musicService
            _analytics = analytics
            _playerManager = playerManager
        End Sub
        
        <Command("status")>
        Public Async Function StatusAsync() As Task
            Dim self As Process = Process.GetCurrentProcess()
            Dim guilds As IReadOnlyCollection(Of IGuild) = Await Context.Client.GetGuildsAsync()
            
            Dim channelCount = 0
            Dim userCount = 0
            Dim commandsExecuted = 0
            Dim installedCommands As Integer = _commandService.Commands.Count()
            
            For Each guild As IGuild In guilds
                channelCount += (Await guild.GetChannelsAsync()).Count
                userCount += (await guild.GetUsersAsync()).Count
            Next
            
            For Each command In _analytics.UsedCommands
                commandsExecuted += command.Value
            Next
            
            Dim output As New StringBuilder
            
            output.AppendLine("Railgun System Status") _
                .AppendLine() _
                .AppendFormat("Messages Intercepted : {0} ({1}/sec)", _analytics.ReceivedMessages, 
                              Math.Round(_analytics.ReceivedMessages / DateTime.Now.Subtract(
                                  self.StartTime).TotalSeconds, 4)).AppendLine() _
                .AppendFormat("    Messages Updated : {0} ({1}/sec)", _analytics.UpdatedMessages, 
                              Math.Round(_analytics.UpdatedMessages / DateTime.Now.Subtract(
                                  self.StartTime).TotalSeconds, 4)).AppendLine() _
                .AppendFormat("  Messages Destroyed : {0} ({1}/sec)", _analytics.DeletedMessages, 
                              Math.Round(_analytics.DeletedMessages / DateTime.Now.Subtract(
                                  self.StartTime).TotalSeconds, 4)).AppendLine() _
                .AppendFormat("  Commands Available : {0}", installedCommands).AppendLine() _
                .AppendFormat("   Commands Executed : {0}", commandsExecuted).AppendLine() _
                .AppendFormat("       Music Streams : {0}", _playerManager.ActivePlayers.Count).AppendLine() _
                .AppendLine() _
                .AppendFormat("      Client Latency : {0}ms", _client.Latency).AppendLine() _
                .AppendFormat("    Connected Shards : {0}", _client.Shards.Count).AppendLine() _
                .AppendFormat("   Connected Servers : {0}", guilds.Count).AppendLine() _
                .AppendFormat("      Total Channels : {0}", channelCount).AppendLine() _
                .AppendFormat("         Total Users : {0}", userCount).AppendLine() _
                .AppendFormat("    Music Repository : {0} ({1} GB)", (Await _musicService.GetAllSongsAsync()).Count(), 
                              Math.Round((((Await _musicService.GetTotalBytesUsedAsync()) _
                                           / 1024) / 1024) / 1024, 2)).AppendLine() _
                .AppendLine() _
                .AppendFormat("       Avg. Channels : {0}/server", Math.Round(channelCount / guilds.Count, 
                                                                              0)).AppendLine() _
                .AppendFormat("          Avg. Users : {0}/server", Math.Round(userCount / guilds.Count, 
                                                                              0)).AppendLine() _
                .AppendLine() _
                .AppendFormat("          Started At : {0}", self.StartTime).AppendLine() _
                .AppendFormat("            CPU Time : {0}", self.TotalProcessorTime).AppendLine() _
                .AppendFormat("             Threads : {0}", self.Threads.Count).AppendLine() _
                .AppendFormat("     Physical Memory : {0} MB", Math.Round((self.WorkingSet64 / 1024.0) / 1024, 
                                                                          2)).AppendLine() _
                .AppendFormat("        Paged Memory : {0} MB", Math.Round((self.PagedMemorySize64 / 1024.0) / 1024, 
                                                                          2)).AppendLine() _
                .AppendLine() _
                .AppendLine("End of Report!")
            
            Await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
        <Command("config")>
        Public Async Function ConfigAsync() As Task
            Dim output As New StringBuilder
            
            output.AppendLine("TreeDiagram Configuration Report") _
                .AppendLine().AppendLine("Server/Guild Configurations :") _
                .AppendFormat("Anti-Caps : {0}", await _dbContext.FilterCapses.CountAsync()).AppendLine() _
                .AppendFormat(" Anti-Url : {0}", await _dbContext.FilterUrls.CountAsync()).AppendLine() _
                .AppendFormat("     Bite : {0}", await _dbContext.FunBites.CountAsync()).AppendLine() _
                .AppendFormat("      RST : {0}", await _dbContext.FunRsts.CountAsync()).AppendLine() _
                .AppendFormat("  Command : {0}", await _dbContext.ServerCommands.CountAsync()) _
                .AppendFormat("JoinLeave : {0}", await _dbContext.ServerJoinLeaves.CountAsync()).AppendLine() _
                .AppendFormat("  Mention : {0}", await _dbContext.ServerMentions.CountAsync()).AppendLine() _
                .AppendFormat("    Music : {0}", await _dbContext.ServerMusics.CountAsync()).AppendLine() _
                .AppendFormat("  Warning : {0}", await _dbContext.ServerWarnings.CountAsync()).AppendLine() _
                .AppendLine().AppendLine("User Configurations :") _
                .AppendFormat("  Mention : {0}", await _dbContext.UserMentions.CountAsync()).AppendLine() _
                .AppendFormat("  Command : {0}", await _dbContext.UserCommands.CountAsync()).AppendLine() _
                .AppendLine() _
                .AppendLine("End of Report!")
            
            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
        <Command("dev")>
        Public Async Function DeveloperAsync() As Task
            Dim output As New StringBuilder
            
            output.AppendFormat("Railgun has been written by {0}.", Format.Bold("ComputerMaster1st#6458")).AppendLine() _
                .AppendFormat("If you have any problems, issues, suggestions, etc, {0} can be found on this discord: {1}", 
                              Format.Bold("ComputerMaster1st"), Format.Bold("<https://discord.gg/Czw5ffx>")).AppendLine() _
                .AppendLine() _
                .AppendFormat("Would you like to support Railgun even further? {0}", 
                              Format.Bold("<https://www.buymeacoffee.com/computermaster1>"))

            await ReplyAsync(output.ToString())
        End Function
        
        <Command("commands")>
        Public Async Function CommandAnalyticsAsync() As Task
            Dim commands = _analytics.UsedCommands.OrderByDescending(Function(r) r.Value)
            Dim count = 20
            Dim output As New StringBuilder
            
            output.AppendFormat("Railgun Top {0} Command Analytics:", count).AppendLine().AppendLine()
            
            For Each command In commands
                output.AppendFormat("{0} <= {1}", command.Value, command.Key).AppendLine()
                count -= 1
                If count < 1 Then Exit For
            Next
            
            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
        <Command("admins")>
        Public Async Function AdminsAsync() As Task
            Dim guild As IGuild = Await Context.Client.GetGuildAsync(_config.DiscordConfig.MasterGuildId)
            Dim master As IGuildUser = Await guild.GetUserAsync(_config.DiscordConfig.MasterAdminId)
            Dim output As new StringBuilder
            
            output.AppendFormat(Format.Bold("{0}#{1}") + " is Railgun's Master Admin.", master.Username, 
                                master.DiscriminatorValue).AppendLine()
            
            If _config.DiscordConfig.OtherAdmins.Count > 0
                output.AppendLine("Other Admins:")
                
                For Each adminId as ULong in _config.DiscordConfig.OtherAdmins
                    Dim admin As IGuildUser = Await guild.GetUserAsync(adminId)
                    
                    If admin Is Nothing
                        output.AppendFormat(Format.Bold("{0}"), adminId).AppendLine()
                        Continue For
                    End If
                    
                    output.AppendFormat(Format.Bold("{0}#{1}"), admin.Username, admin.DiscriminatorValue).AppendLine()
                Next
            End If
            
            await ReplyAsync(output.ToString())
        End Function
        
        <Command("timers")>
        Public Async Function TimersAsync() As Task
            Dim output As New StringBuilder
            
            output.AppendLine("TreeDiagram Timers Status") _
                .AppendLine() _
                .AppendFormat("Remind Me : {0}", _dbContext.TimerRemindMes.CountAsync())
            
            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
    End Class
    
End NameSpace