Imports System
Imports Discord
Imports Discord.WebSocket
Imports RailgunVB.Core
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Managers

Module Program
    Private _config As MasterConfig
    Private _client As DiscordShardedClient
    Private _sysManager As Initializer
    
    Sub Main(args As String())
        RunAsync().GetAwaiter().GetResult()
    End Sub
    
    Private Async Function RunAsync() As Task
        Console.WriteLine("Starting TreeDiagram Operating System...")
        
        _config = Await MasterConfig.LoadAsync()
        
        _client = New DiscordShardedClient(New DiscordSocketConfig() With {
            .LogLevel = LogSeverity.Info,
            .DefaultRetryMode = RetryMode.AlwaysRetry
        })
        
        _sysManager = New Initializer(_config, _client)
        Await _sysManager.InitializeCommandsAsync()
        
        Await _client.LoginAsync(TokenType.Bot, _config.DiscordConfig.Token)
        Await _client.StartAsync()
        Await Task.Delay(-1)
    End Function
End Module
