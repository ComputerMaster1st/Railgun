Imports Discord.Commands
Imports Discord.WebSocket
Imports RailgunVB.Core.Logging

Namespace Core.Managers
    
    Public Class CommandManager
    
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _log As Log
        
        Private ReadOnly _client As DiscordShardedClient
        Private ReadOnly _commandService As CommandService

        Public Sub New(config As MasterConfig, client As DiscordShardedClient)
            _config = config
            _client = client
            
            _log = New Log(_config, _client)
            _commandService = New CommandService(New CommandServiceConfig() With {
                                                    .DefaultRunMode = RunMode.Async
                                                })
        End Sub
        
    End Class
    
End NameSpace