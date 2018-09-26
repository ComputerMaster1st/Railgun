Imports System.IO
Imports System.Text
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.EntityFrameworkCore
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Models.Server
Imports TreeDiagram.Models.User

Namespace Core.Managers
    
    Public Class CommandManager
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _log As Log
        Private ReadOnly _analytics As Analytics
        Private ReadOnly _filterManager As FilterManager
        Private ReadOnly _dbContext As TreeDiagramContext
        
        Private WithEvents _client As DiscordShardedClient
        Private WithEvents _commandService As CommandService
        
        Private ReadOnly _services As IServiceProvider

        Public Sub New(config As MasterConfig, log As Log, analytics As Analytics, filterManager As FilterManager, 
                       dbContext As TreeDiagramContext, client As DiscordShardedClient,
                       commandService As CommandService,  services As IServiceProvider)
            _config = config
            _log = log
            _analytics = analytics
            _filterManager = filterManager
            _dbContext = dbContext
            _client = client
            _commandService = commandService
            _services = services
        End Sub
        
        Private Async Function AutoDeleteFilterMsgAsync(userMsg As IUserMessage, filterMsg As IUserMessage) As Task
            Try
                Await userMsg.DeleteAsync()
                
                _analytics.DeletedMessages += 1
                
                Await Task.Delay(5000)
                Await filterMsg.DeleteAsync()
            Catch
            End Try
        End Function
        
        Private Async Function ReceiveMessageAsync(sMessage As SocketMessage) As Task Handles _client.MessageReceived
            If sMessage Is Nothing OrElse 
               TypeOf sMessage Is SocketSystemMessage OrElse 
               TypeOf sMessage.Channel Is SocketDMChannel OrElse 
               TypeOf sMessage.Channel Is SocketGroupChannel Then Return
            
            Await ProcessMessageAsync(sMessage)
        End Function
        
        Private Async Function UpdateMessageAsync(oldMessage As Cacheable(Of IMessage, ULong), 
                                                  newMessage As SocketMessage, channel As ISocketMessageChannel
                                                 ) As Task Handles _client.MessageUpdated
            Await ProcessMessageAsync(newMessage)
        End Function
        
        Private Async Function ProcessMessageAsync(sMessage As SocketMessage) As Task
            Try
                Dim msg As IUserMessage = sMessage
                Dim guild As IGuild = CType(msg.Channel, ITextChannel).Guild
                Dim filterMsg As IUserMessage = Await _filterManager.ApplyFilterAsync(msg)
                
                If filterMsg IsNot Nothing
                    Await Task.Run(New Action(AddressOf AutoDeleteFilterMsgAsync(msg, filterMsg)))
                    Return
                End If
                
                Dim argPos As Integer = 0
                Dim sCommand As ServerCommand = Await _dbContext.ServerCommands.GetAsync(guild.Id)
                
                If ((sCommand Is Nothing OrElse Not (sCommand.RespondToBots)) AndAlso msg.Author.IsBot) OrElse 
                   msg.Author.IsWebhook Then Return
                
                Dim uCommand As UserCommand = Await _dbContext.UserCommands.GetAsync(msg.Author.Id)
                
                If msg.HasStringPrefix(_config.DiscordConfig.Prefix, argPos)
                    Await ExecuteCommandAsync(_config.DiscordConfig.Prefix, msg, argPos, sCommand)
                ElseIf msg.HasMentionPrefix(_client.CurrentUser, argPos)
                    Await ExecuteCommandAsync(_client.CurrentUser.Mention, msg, argPos, sCommand)
                ElseIf (sCommand IsNot Nothing AndAlso Not (String.IsNullOrEmpty(sCommand.Prefix))) AndAlso 
                       msg.HasStringPrefix(sCommand.Prefix, argPos)
                    Await ExecuteCommandAsync(uCommand.Prefix, msg, argPos, sCommand)
                ElseIf (uCommand IsNot Nothing AndAlso Not (String.IsNullOrEmpty(uCommand.Prefix))) AndAlso 
                       msg.HasStringPrefix(uCommand.Prefix, argPos)
                    Await ExecuteCommandAsync(uCommand.Prefix, msg, argPos, sCommand)
                End If
            Catch e As Exception
                _log.LogToConsoleAsync(New LogMessage(
                    LogSeverity.Warning, "Command", "Unexpected Exception!", e)).GetAwaiter()
            End Try
        End Function
        
        Private Async Function ExecuteCommandAsync(prefix As String, msg As IUserMessage, 
                                                   argPos as Integer, data As ServerCommand) As Task
            If msg.Content.Length <= prefix.Length
                Return
            ElseIf msg.Content(argPos) = " "c
                argPos += 1
            End If
            
            Dim cmdContext As New CommandContext(_client, msg)
            Dim result As IResult = Await _commandService.ExecuteAsync(cmdContext, argPos, _services)
            
            If data IsNot Nothing AndAlso data.DeleteCmdAfterUse Then Await msg.DeleteAsync()
            If result.IsSuccess Then Return
            
            Dim cmdError As String = $"Command Error: {result.ErrorReason}"
            Dim tc As ITextChannel = cmdContext.Channel
            
            Select result.Error
                Case CommandError.UnmetPrecondition
                    Await tc.SendMessageAsync(cmdError)
                    Exit Select
            End Select
        End Function
        
        Private Async Function CommandLogAsync(msg As LogMessage) As Task Handles _commandService.Log
            Dim cmdEx As CommandException = msg.Exception
            
            If cmdEx Is Nothing Then Return
            
            Dim cmdContext As ICommandContext = cmdEx.Context
            Dim command As CommandInfo = cmdEx.Command
            Dim output As New StringBuilder
            
            output.AppendFormat("<{0} ({1})>", cmdContext.Guild.Name, cmdContext.Guild.Id).AppendLine() _
                .AppendFormat("---- Command : {0}", command.Aliases(0)).AppendLine() _
                .AppendFormat("---- Content : {0}", cmdContext.Message.Content).AppendLine() _
                .AppendLine("---- Result  : FAILURE").AppendLine() _
                .AppendLine(cmdEx.ToString())
            
            If output.Length > 2000
                Const filename = "cmdmngr-error.txt"
                
                Await File.WriteAllTextAsync(filename, output.ToString())
                Await _log.LogToBotLogAsync(output.ToString(), BotLogType.CommandManager, True, filename)
                
                File.Delete(filename)
                Return
            End If
            
            Await _log.LogToBotLogAsync(output.ToString(), BotLogType.CommandManager, true)
            Await cmdContext.Channel.SendMessageAsync($"{Format.Bold("OH NO!") _
                    } Something bad has happened inside of me! I've alerted my developer about the problem. I hope they'll get me all patched up soon!")
        End Function
        
    End Class
    
End NameSpace