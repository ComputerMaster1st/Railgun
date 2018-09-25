Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Logging

Namespace Core.Managers
    
    Public Class CommandManager
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _log As Log
        Private ReadOnly _analytics As Analytics
        
        Private WithEvents _client As DiscordShardedClient
        Private ReadOnly _commandService As CommandService

        Public Sub New(config As MasterConfig, 
                       log As Log, 
                       analytics As Analytics,
                       client As DiscordShardedClient, 
                       commandService As CommandService
                      )
            _config = config
            _log = log
            _analytics = analytics
            _client = client
            _commandService = commandService
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
                                                  newMessage As SocketMessage, 
                                                  channel As ISocketMessageChannel
                                                 ) As Task Handles _client.MessageUpdated
            Await ProcessMessageAsync(newMessage)
        End Function
        
        Private Async Function ProcessMessageAsync(sMessage As SocketMessage) As Task
            Try
                Dim msg As IUserMessage = sMessage
                Dim guild As IGuild = CType(msg.Channel, ITextChannel).Guild
                
            End Try
        End Function
        

'        private async Task ProcessMessageAsync(SocketMessage sMessage) {
'            try {
'                FilterManager mFilter = treeDiagramProvider.GetService<FilterManager>();
'                IUserMessage filterMsg = await mFilter.ApplyFilterAsync(message);
'
'                if (filterMsg != null) {
'                    await Task.Run(new Action(async () => await AutoDeleteFilterMsgAsync(message, filterMsg)));
'                    return;
'                }
'
'                int argPos = 0;
'                VManager<GD_Command> vgCommand = treeDiagramProvider.GetService<VManager<GD_Command>>();
'                GD_Command gCommand = await vgCommand.GetAsync(guild.Id);
'
'                if ((message.Author.IsBot && (gCommand == null || !gCommand.RespondToBots)) || message.Author.IsWebhook) return;
'
'                VManager<GD_Prefix> vgPrefix = treeDiagramProvider.GetService<VManager<GD_Prefix>>();
'                VManager<UD_Prefix> vuPrefix = treeDiagramProvider.GetService<VManager<UD_Prefix>>();
'                GD_Prefix gPrefix = await vgPrefix.GetAsync(guild.Id);
'                UD_Prefix uPrefix = await vuPrefix.GetAsync(guild.Id);
'
'                if (message.HasStringPrefix(config.Prefix, ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) {
'                    await ExecuteCommandAsync(message, argPos, gCommand);
'                } else if (gPrefix != null && !string.IsNullOrEmpty(gPrefix.Prefix) && message.HasStringPrefix(gPrefix.Prefix, ref argPos)) {
'                    await ExecuteCommandAsync(message, argPos, gCommand);
'                } else if (uPrefix != null && !string.IsNullOrEmpty(uPrefix.Prefix) && message.HasStringPrefix(uPrefix.Prefix, ref argPos)) {
'                    await ExecuteCommandAsync(message, argPos, gCommand);
'                }
'            } catch (Exception e) {
'                await log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "UNKNOWN", "Something Unknown Happened!", e));
'            }
'        }
'
'        private async Task ExecuteCommandAsync(IUserMessage message, int argPos, GD_Command data) {
'            if (message.Content.Length <= config.Prefix.Length) return;
'            else if (message.Content[argPos] == ' ') argPos++;
'
'            CommandContext context = new CommandContext(client, message);
'            IResult result = await commandService.ExecuteAsync(context, argPos, treeDiagramProvider);
'
'            if (data != null && data.DeleteCommandAfterUse) await message.DeleteAsync();
'            if (result.IsSuccess) return;            
'
'            string commandError = $"Command Error: {result.ErrorReason}";
'            ITextChannel tc = (ITextChannel)context.Channel;
'
'            switch (result.Error) {
'                case CommandError.UnmetPrecondition:
'                    await tc.SendMessageAsync(commandError);
'                    break;
'                default:
'                    break;
'            }
'        }
'
'        private async Task CommandLogAsync(LogMessage message) {
'            CommandException exception = (CommandException)message.Exception;
'
'            if (exception == null) return;
'
'            ICommandContext context = exception.Context;
'            CommandInfo command = exception.Command;
'            StringBuilder output = new StringBuilder();
'
'            output.AppendFormat("<{0} ({1})>", context.Guild.Name, context.Guild.Id).AppendLine()
'                .AppendFormat("---- Command : {0}", command.Aliases[0]).AppendLine()
'                .AppendFormat("---- Content : {0}", context.Message.Content).AppendLine()
'                .AppendLine("---- Result  : FAILURE").AppendLine()
'                .AppendLine(exception.ToString());
'
'            if (output.Length > 2000) {
'                const string filename = "cmdmngr-error.txt";
'                await File.WriteAllTextAsync(filename, output.ToString());
'                await log.LogToBotLogAsync(output.ToString(), BotLogType.CommandManager, true, filename);
'                File.Delete(filename);
'                return;
'            }
'
'            await log.LogToBotLogAsync(output.ToString(), BotLogType.CommandManager, true);
'            await context.Channel.SendMessageAsync($"{Format.Bold("OH NO!")} Something bad has happened inside of me! I've alerted my developer about the problem. I hope they'll get me all patched up soon!");
'        }
    End Class
    
End NameSpace