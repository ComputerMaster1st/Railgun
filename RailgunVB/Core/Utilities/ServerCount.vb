Imports System.Timers
Imports Discord
Imports Discord.WebSocket
Imports RailgunVB.Core.Configuration

Namespace Core.Utilities
    
    Public Class ServerCount
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _client As DiscordShardedClient
        
        Private WithEvents _timer As New Timer(TimeSpan.FromMinutes(30).TotalMilliseconds)
        
        Public Property PreviousGuildCount As Integer = 0

        Public Sub New(config As MasterConfig, client As DiscordShardedClient)
            _config = config
            _client = client
            
            _timer.AutoReset = True
            _timer.Start()
        End Sub
        
        Private Async Sub UpdateServerCountAsync() Handles _timer.Elapsed
            If PreviousGuildCount = _client.Guilds.Count Then Return
            
            PreviousGuildCount = _client.Guilds.Count
            
            Await _client.SetGameAsync(String.Format(
                "{0}help || {1} Servers!",
                _config.DiscordConfig.Prefix,
                PreviousGuildCount
            ), type := ActivityType.Watching)
        End Sub
        
    End Class
    
End NameSpace