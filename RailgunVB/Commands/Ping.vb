Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket

Namespace Commands
    
    <Group("ping")>
    Public Class Ping
        Inherits ModuleBase
        
        Private ReadOnly _client As DiscordShardedClient
        
        Public Sub New(client As DiscordShardedClient)
            _client = client
        End Sub
    
        <Command>
        Public Function PingAsync() As Task
            Return ReplyAsync($"Discord Client Latency : {Format.Bold(_client.Latency.ToString())}ms")
        End Function
        
    End Class
    
End NameSpace