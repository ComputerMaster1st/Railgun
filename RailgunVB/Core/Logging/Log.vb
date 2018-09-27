Imports System.IO
Imports Discord
Imports Discord.WebSocket
Imports RailgunVB.Core.Configuration

Namespace Core.Logging
    
    Public Class Log
        
        Private ReadOnly _config As Configuration.DiscordConfig
        Private WithEvents _client As DiscordShardedClient
        
        Private Const LogDirectory As String = "logs"
        Private ReadOnly _logFilename As String = $"{LogDirectory}/{DateTime.Today.ToString("yyyy-MM-dd")}.log"

        Public Sub New(config As MasterConfig, client As DiscordShardedClient)
            _config = config.DiscordConfig
            _client = client
            
            If Not (Directory.Exists(LogDirectory)) Then Directory.CreateDirectory(LogDirectory)
        End Sub
        
        Public Async Function LogToBotLogAsync(entry As String, type As BotLogType, 
                                               Optional pingMaster As Boolean = False, 
                                               Optional filename As String = Nothing) As Task
            If _config.MasterGuildId = 0 Then Return
            
            Dim guild As IGuild = Await CType(_client, IDiscordClient).GetGuildAsync(_config.MasterGuildId)
            
            Select type
                Case BotLogType.Common
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.Common, entry, pingMaster, filename)
                    Exit Select
                Case BotLogType.AudioChord
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.AudioChord, entry, pingMaster, filename)
                    Exit Select
                Case BotLogType.CommandManager
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.CommandMngr, entry, pingMaster, filename)
                    Exit Select
                Case BotLogType.GuildManager
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.GuildMngr, entry, pingMaster, filename)
                    Exit Select
                Case BotLogType.MusicManager
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.MusicMngr, entry, pingMaster, filename)
                    Exit Select
                Case BotLogType.MusicPlayer
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.MusicPlayer, entry, pingMaster, filename)
                    Exit Select
                Case BotLogType.TaskScheduler
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.TaskSch, entry, pingMaster, filename)
                    Exit Select
                Case BotLogType.TimerManager
                    Await SendBotLogAsync(guild, type, _config.BotLogChannels.TimerMngr, entry, pingMaster, filename)
                    Exit Select
            End Select
        End Function
        
        Public Async Function LogToConsoleAsync(message As LogMessage) As Task Handles _client.Log
            Select message.Severity
                Case LogSeverity.Info
                    Console.ForegroundColor = ConsoleColor.Blue
                    Exit Select
                Case LogSeverity.Critical
                    Console.ForegroundColor = ConsoleColor.DarkRed
                    Exit Select
                Case LogSeverity.Error
                    Console.ForegroundColor = ConsoleColor.Red
                    Exit Select
                Case LogSeverity.Warning
                    Console.ForegroundColor = ConsoleColor.Yellow
                    Exit Select
                Case Else
                    Console.ForegroundColor = ConsoleColor.Magenta
                    Exit Select
            End Select
            
            Console.WriteLine(message.ToString())
            Await WriteToLogFileAsync(message.ToString())
        End Function
        
        Private Async Function SendBotLogAsync(guild As IGuild, type As BotLogType, channelId As ULong,
                                               entry As String, pingMaster As Boolean, filename As String) As Task
            Dim output As String = 
                $"[ {DateTime.Now.ToString("HH:mm:ss")} ] || { _ 
                    If(Not (type = BotLogType.Common), $"{type} ||", "")} {entry}"
            
            Try
                Dim tc As ITextChannel
                
                If Not (channelId = 0) Then
                    tc = Await guild.GetTextChannelAsync(channelId)
                ElseIf Not (_config.BotLogChannels.Common = 0) Then
                    tc = Await guild.GetTextChannelAsync(_config.BotLogChannels.Common)
                Else
                    Return
                End If
                
                Dim pingMasterStr As String = If(pingMaster, $"<@!{_config.MasterAdminId}>", String.Empty)
                
                If Not (String.IsNullOrEmpty(filename))
                    Await tc.SendFileAsync(filename, $"Error! Refer to file, {pingMasterStr}!")
                    File.Delete(filename)
                    Return
                End If
                
                Await tc.SendMessageAsync(Format.Code(output) + pingMasterStr)
            Catch
            End Try
            
            Await WriteToLogFileAsync(output)
        End Function
        
        Private Async Function WriteToLogFileAsync(entry As String) As Task
            Using logFile = File.AppendText(_logFilename)
                Await logFile.WriteLineAsync(entry)
            End Using
        End Function
        
    End Class
    
End NameSpace