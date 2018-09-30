Imports System.Reflection
Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports Discord.WebSocket
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Api
Imports RailgunVB.Core.Api.Youtube
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Filters
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Configuration

Namespace Core
    
    Public Class Initializer
    
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
            
            Dim postgreConfig As DatabaseConfig = _config.PostgreDatabaseConfig
            Dim mongoConfig As DatabaseConfig = _config.MongoDatabaseConfig
            
            _services = New ServiceCollection() _
                .AddSingleton(_config) _
                .AddSingleton(_log) _
                .AddSingleton(_serverCount) _
                .AddSingleton(_client) _
                .AddSingleton(Of IDiscordClient)(_client) _
                .AddSingleton(_commandService) _
                .AddTransient(Of TreeDiagramContext)(function(provider) New TreeDiagramContext(
                    New PostgresConfig(
                        postgreConfig.Hostname, 
                        postgreConfig.Username, 
                        postgreConfig.Password, 
                        postgreConfig.Database))) _
                .AddSingleton(New MusicService(New MusicServiceConfig() With {
                    .Hostname = mongoConfig.Hostname,
                    .Username = mongoConfig.Username,
                    .Password = mongoConfig.Password,
                    .EnableResync = True
                })) _
                .AddSingleton(Of Analytics) _
                .AddSingleton(Of CommandUtils) _
                .AddSingleton(Of Events) _
                .AddSingleton(Of RandomCat) _
                .AddSingleton(Of CommandManager) _
                .AddSingleton(Of FilterManager) _
                .AddSingleton(Of MusicManager) _
                .AddSingleton(Of PlayerManager) _
                .AddSingleton(Of TimerManager) _
                .AddSingleton(Of AntiCaps) _
                .AddSingleton(Of AntiUrl) _
                .AddTransient(Of YoutubeSearch) _
                .BuildServiceProvider()
            
            AddHandler TaskScheduler.UnobservedTaskException, Async Sub(s, a) Await UnobservedTaskAsync(a) 
        End Sub

        Public Async Function InitializeCommandsAsync() As Task
            _services.GetService(Of Analytics)()
            _services.GetService(Of Events)()
            _services.GetService(Of CommandManager)()
            _services.GetService(Of MusicManager)()
            _services.GetService(Of AntiCaps)()
            _services.GetService(Of AntiUrl)()
            
            Await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", "TreeDiagram Ready!"))
            Await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services)
            Await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, "System", 
                $"{_commandService.Commands.Count} Commands Loaded!"))
        End Function
        
        Private Async Function UnobservedTaskAsync(args As UnobservedTaskExceptionEventArgs) As Task
            args.SetObserved()
            
            Await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Error, "System", 
                "An unobserved task threw an exception!", args.Exception))
            
            Dim output As New StringBuilder
            
            output.AppendLine("An unobserved task threw an exception!") _
                .AppendLine(args.Exception.ToString())
            
            If output.Length < 1950 Then
                Await _log.LogToBotLogAsync(output.ToString(), BotLogType.TaskScheduler)
            Else 
                Await _log.LogToBotLogAsync("An unobserved task threw an exception! Refer to log files!", 
                    BotLogType.TaskScheduler)
            End If
        End Function
        
    End Class
    
End NameSpace