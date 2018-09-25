Imports Discord.WebSocket

Namespace Core.Managers
    
    Public Class CommandManager
    
        Private _config As MasterConfig
        Private _client As DiscordShardedClient

        Public Sub New(config As MasterConfig, client As DiscordShardedClient)
            _config = config
            _client = client
        End Sub
        
    End Class
    
End NameSpace