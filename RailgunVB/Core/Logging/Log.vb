Imports System.IO
Imports Discord
Imports Discord.WebSocket

Namespace Core.Logging
    
    Public Class Log
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly WithEvents _client As DiscordShardedClient
        
        Private ReadOnly _logFilename As String = String.Format("logs/{0}.log", DateTime.Today.ToString("yyyy-MM-dd"))

        Public Sub New(config As MasterConfig, client As DiscordShardedClient)
            _config = config
            _client = client
        End Sub
        
        Public Async Function LogToBotLogAsync(entry As String, type As BotLogType, Optional pingMaster As Boolean = False, Optional filename As String = String.Empty) As Task
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
            Console.WriteLine(message.ToString())
            Await WriteToLogFileAsync(message.ToString())
        End Function
        
        Private Async Function SendBotLogAsync(guild As IGuild, type As BotLogType, channelId As ULong, entry As String, pingMaster As Boolean, filename As String) As Task
            Dim output As String = String.Format("[ {0} ] || {1} {2}",
                                                 DateTime.Now.ToString("HH:mm:ss"),
                                                 If(Not (type.Equals(BotLogType.Common)), String.Format("{0} ||", type), "")
                                                )
            
            Try
                Dim tc As ITextChannel
                
                If Not (channelId = 0) Then
                    tc = Await guild.GetTextChannelAsync(channelId)
                ElseIf Not (_config.BotLogChannels.Common = 0) Then
                    tc = Await guild.GetTextChannelAsync(_config.BotLogChannels.Common)
                Else
                    Return
                End If
                
                Dim pingMasterStr As String = If(pingMaster, String.Format("<@!{0}>", _config.MasterAdminId, String.Empty))
                
                If Not (String.IsNullOrEmpty(filename))
                    Await tc.SendFileAsync(filename, String.Format("Error! Refer to file, {0}!", pingMasterStr))
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