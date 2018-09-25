Imports Discord.Commands
Imports Discord.WebSocket
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Logging

Namespace Core.Managers
    
    Public Class CommandManager
        
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _log As Log
        
        Private WithEvents _client As DiscordShardedClient
        Private ReadOnly _commandService As CommandService

        Public Sub New(config As MasterConfig, log As Log, client As DiscordShardedClient, commandService As CommandService)
            _config = config
            _log = log
            _client = client
            _commandService = commandService
        End Sub
        
    End Class
    
End NameSpace