Imports AudioChord
Imports Discord
Imports Discord.WebSocket
Imports RailgunVB.Core.Configuration
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Utilities
Imports TreeDiagram
Imports TreeDiagram.Enums
Imports TreeDiagram.Models.Server
Imports TreeDiagram.Models.User

Namespace Core
    
    Public Class Events
    
        Private ReadOnly _config As MasterConfig
        Private ReadOnly _log As Log
        Private ReadOnly _commandUtils As CommandUtils
        Private ReadOnly _serverCount As ServerCount

        Private WithEvents _client As DiscordShardedClient
        
        Private ReadOnly _dbContext As TreeDiagramContext
        Private ReadOnly _musicService As MusicService
        
        Private ReadOnly _playerManager As PlayerManager
        Private ReadOnly _timerManager As TimerManager
        
        Private _initialized As Boolean = False
        Private ReadOnly _shardsReady As New Dictionary(Of Integer, Boolean)
        
        Public Sub New(config As MasterConfig, log As Log, commandUtils As CommandUtils, serverCount As ServerCount,
                       client As DiscordShardedClient, dbContext As TreeDiagramContext, musicService As MusicService,
                       playerManager As PlayerManager, timerManager As TimerManager)
            _config = config
            _log = log
            _commandUtils = commandUtils
            _serverCount = serverCount
            
            _client = client
            
            _dbContext = dbContext
            _musicService = musicService
            
            _playerManager = playerManager
            _timerManager = timerManager
        End Sub
        
        Private Async Function JoinedGuildAsync(sGuild As SocketGuild) As Task Handles _client.JoinedGuild
            Await _log.LogToBotLogAsync($"<{sGuild.Name} ({sGuild.Id})> Joined", BotLogType.GuildManager)
        End Function
        
        Private Async Function LeftGuildAsync(sGuild As SocketGuild) As Task Handles _client.LeftGuild
            Await Task.Run(Async Sub() Await DedicatedLeftGuildAsync(sGuild))
        End Function
        
        Private Async Function DedicatedLeftGuildAsync(sGuild As SocketGuild) As Task
            If _playerManager.IsCreated(sGuild.Id) Then _playerManager.GetPlayer(sGuild.Id).CancelStream()
            
            Await _musicService.CancelGuildMusicProcessingAsync(sGuild.Id)
            Await _dbContext.DeleteGuildDataAsync(sGuild.Id)
            Await _log.LogToBotLogAsync($"<{sGuild.Name} ({sGuild.Id})> Left", BotLogType.GuildManager)
        End Function
        
        Private Async Function UserJoinedAsync(sUser As SocketGuildUser) As Task Handles _client.UserJoined
            Dim sJoinLeave As ServerJoinLeave = Await _dbContext.ServerJoinLeaves.GetAsync(sUser.Guild.Id)
            
            If sJoinLeave Is Nothing Then Return
            
            Dim notification As String = sJoinLeave.GetMessage(MsgType.Join).Replace("<server>", sUser.Guild.Name) _
                .Replace("<user>", Await _commandUtils.GetUsernameOrMentionAsync(sUser))
            
            Await SendJoinLeaveMessageAsync(sJoinLeave, sUser, notification)
        End Function
        
        Private Async Function UserLeaveAsync(sUser As SocketGuildUser) As Task Handles _client.UserLeft
            Dim sJoinLeave As ServerJoinLeave = Await _dbContext.ServerJoinLeaves.GetAsync(sUser.Guild.Id)
            
            If sJoinLeave Is Nothing Then Return
            
            Dim notification As String = sJoinLeave.GetMessage(MsgType.Leave)
            
            If Not (String.IsNullOrEmpty(notification)) Then notification = notification _
                .Replace("<user>", Await _commandUtils.GetUsernameOrMentionAsync(sUser))
            
            Await SendJoinLeaveMessageAsync(sJoinLeave, sUser, notification)
        End Function
        
        Private Async Function SendJoinLeaveMessageAsync(data As ServerJoinLeave, user As IGuildUser, 
                                                         message As String) As Task
            If String.IsNullOrEmpty(message)
                Return
            ElseIf data.SendToDM
                Try
                    Dim dm As IDMChannel = Await user.GetOrCreateDMChannelAsync()
                    
                    Await dm.SendMessageAsync(message)
                Catch
                End Try
            ElseIf data.ChannelId <> 0
                Dim tc As ITextChannel = Await user.Guild.GetTextChannelAsync(data.ChannelId)
                
                If tc IsNot Nothing Then Await tc.SendMessageAsync(message)
            End If
        End Function
        
        Private Async Function UserVoiceStateUpdatedAsync(sUser As SocketUser, before As SocketVoiceState, 
                                                          after As SocketVoiceState) As Task _
                                                          Handles _client.UserVoiceStateUpdated
            If sUser.IsBot OrElse after.VoiceChannel Is Nothing Then 
                Return
            End If
            
            Dim guild As IGuild = after.VoiceChannel.Guild
            Dim user As IGuildUser = Await guild.GetUserAsync(sUser.Id)
            
            If _playerManager.IsCreated(guild.Id) OrElse user.VoiceChannel Is Nothing Then Return
            
            Dim sMusic As ServerMusic = Await _dbContext.ServerMusics.GetAsync(guild.Id)
            
            If sMusic Is Nothing Then Return
            
            Dim tc As ITextChannel = If(sMusic.AutoTextChannel <> 0, Await guild.GetTextChannelAsync(sMusic.AutoTextChannel), Nothing)
            Dim vc As IVoiceChannel = user.VoiceChannel
            
            If vc.Id = sMusic.AutoVoiceChannel AndAlso tc IsNot Nothing Then _ 
                Await _playerManager.CreatePlayerAsync(user, vc, tc, True)
        End Function
        
        Private Async Function ShardReadyAsync(sClient As DiscordSocketClient) As Task Handles _client.ShardReady
            If _playerManager.ActivePlayers.Count > 0
                For Each player In _playerManager.ActivePlayers
                    player.Value.Item2.CancelStream()
                Next
            End If
            
            If Not (_shardsReady.ContainsKey(sClient.ShardId)) 
                _shardsReady.Add(sClient.ShardId, False)
            Else 
                _shardsReady(sClient.ShardId) = True
            End If
            
            Await _log.LogToConsoleAsync(new LogMessage(LogSeverity.Info, $"SHARD {sClient.ShardId}", 
                $"Shard {If(_shardsReady(sClient.ShardId), "Re-", "")}Connected! ({sClient.Guilds.Count} Servers)"))
            Await _timerManager.Initialize()
            
            If _initialized 
                Return
            ElseIf _shardsReady.Count < _client.Shards.Count
                Return
            End If
            
            _initialized = True
            _serverCount.PreviousGuildCount = _client.Guilds.Count
            
            Await _client.SetGameAsync($"{_config.DiscordConfig.Prefix}help || {_client.Guilds.Count} Servers!", 
                                       type := ActivityType.Watching)
        End Function
            
    End Class
    
End NameSpace