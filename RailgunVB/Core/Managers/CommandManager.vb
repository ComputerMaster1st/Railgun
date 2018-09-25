Imports System.Reflection
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Utilities

Namespace Core.Managers
    
    Public Class CommandManager
    
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _log As Log
        Private ReadOnly _serverCount As ServerCount
        
        Private ReadOnly _client As DiscordShardedClient
        Private ReadOnly _commandService As CommandService
        
        Private ReadOnly _services As IServiceProvider

        Public Sub New(config As MasterConfig, client As DiscordShardedClient)
            _config = config
            _client = client
            
            _commandService = New CommandService(New CommandServiceConfig() With {
                                                    .DefaultRunMode = RunMode.Async
                                                    })
            
            _log = New Log(_config, _client)
            _serverCount = New ServerCount(_config, _client)
            
            _services = New ServiceCollection() _
                .AddSingleton(_config) _
                .AddSingleton(_log) _
                .AddSingleton(_serverCount) _
                .AddSingleton(_client) _
                .AddSingleton(Of IDiscordClient)(_config) _
                .AddSingleton(_commandService) _
                .BuildServiceProvider()
        End Sub
        
        Public Async Function InitializeCommandsAsync() As Task
            Await _log.LogToConsoleAsync(new LogMessage(
                LogSeverity.Info,
                "Services",
                "Ready!"
            ))
            
            Await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services)
            
            Await _log.LogToConsoleAsync(new LogMessage(
                LogSeverity.Info,
                "CommandMngr",
                String.Format("{0} Loaded!", _commandService.Commands.Count)
            ))
        End Function
        
    End Class
    
End NameSpace