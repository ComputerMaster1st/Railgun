Imports System
Imports Discord
Imports Discord.WebSocket
Imports RailgunVB.Core
Imports RailgunVB.Core.Managers

Module Program
    Private _config As MasterConfig
    Private _client As DiscordShardedClient
    Private _cmdManager As CommandManager
    
    Sub Main(args As String())
        RunAsync().GetAwaiter().GetResult()
    End Sub
    
    Private Async Function RunAsync() As Task
        Console.WriteLine("Starting TreeDiagram Operating System...")
        
        _config = Await MasterConfig.LoadAsync()
        
        _client = New DiscordShardedClient(New DiscordSocketConfig() With {
            .LogLevel = LogSeverity.Verbose,
            .DefaultRetryMode = RetryMode.AlwaysRetry
        })
        
        _cmdManager = New CommandManager(_config, _client)
        Await _cmdManager.InitializeCommandsAsync()
        
        Await _client.LoginAsync(TokenType.Bot, _config.DiscordToken)
        Await _client.StartAsync()
        Await Task.Delay(-1)
    End Function
End Module
