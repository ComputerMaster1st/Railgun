Imports System.Text
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports RailgunVB.Core.Logging

Namespace Core
    
    Public Class Analytics
        
        Private ReadOnly _log As Log
        
        Private WithEvents _client As DiscordShardedClient
        Private WithEvents _commandService As CommandService
        
        Public ReadOnly Property ReceivedMessages As ULong = 0
        Public ReadOnly Property UpdatedMessages As ULong = 0
        Public Property DeletedMessages As ULong = 0
        
        Public ReadOnly Property UsedCommands As New Dictionary(Of String, Integer)

        Public Sub New(log As Log, client As DiscordShardedClient, commandService As CommandService)
            _log = log
            _client = client
            _commandService = commandService
        End Sub
        
        Private Function MessageReceived(message As SocketMessage) As Task Handles _client.MessageReceived
            _ReceivedMessages += 1
            Return Task.CompletedTask
        End Function
        
        Private Function MessageReceived(oldMessage As Cacheable(Of IMessage, ULong), 
                                         newMessage As SocketMessage, 
                                         channel As ISocketMessageChannel
                                         ) As Task Handles _client.MessageUpdated
            _UpdatedMessages += 1
            Return Task.CompletedTask
        End Function
        
        Private Async Function ExecutedCommandAsync(command As CommandInfo, 
                                                    context As ICommandContext, 
                                                    result As IResult
                                                   ) As Task Handles _commandService.CommandExecuted
            If Not (result.IsSuccess) Then Return
            
            Dim cmdString As String = command.Aliases(0)
            Dim guild As IGuild = context.Guild
            Dim output As New StringBuilder
            
            If UsedCommands.ContainsKey(cmdString) Then 
                UsedCommands(cmdString) += 1
            Else 
                UsedCommands.Add(cmdString, 1)
            End If
            
            output.AppendFormat("<{0} <{1}>>", guild.Name, guild.Id).AppendLine() _
                .AppendFormat("---- Command : {0}", cmdString).AppendLine()
            
            If context.Message.Content.Length < 250 Then _
                output.AppendFormat("---- Content : {0}", context.Message.Content).AppendLine()
            
            output.AppendLine("---- Result  : Completed")
            
            Await _log.LogToBotLogAsync(output.ToString(), BotLogType.CommandManager)
        End Function
        
    End Class
    
End NameSpace