Imports System.IO
Imports Newtonsoft.Json
Imports RailgunVB.Core.Logging

Namespace Core
    
    <JsonObject(MemberSerialization.Fields)>
    Public Class MasterConfig
        <JsonIgnore> Private Const Filename As String = "masterconfig.json"
        
        Public ReadOnly Property DiscordToken As String
        Public Property DiscordPrefix As String
        Public ReadOnly Property MasterAdminId As ULong
        Public ReadOnly Property MasterGuildId As ULong = 0
        Public ReadOnly Property BotLogChannels As BotLogChannels = New BotLogChannels
        Public ReadOnly Property GoogleApiToken As String
        Public ReadOnly Property RandomCatApiToken As String
        Public ReadOnly Property OtherAdmins As List(Of ULong) = New List(Of ULong)()

        Public Sub New (discordToken As String,
                        discordPrefix As String,
                        masterAdminId As ULong,
                        googleApiToken As String,
                        randomCatApiToken As String)
            
            Me.DiscordToken = discordToken
            Me.DiscordPrefix = discordPrefix
            Me.MasterAdminId = masterAdminId
            Me.GoogleApiToken = googleApiToken
            Me.RandomCatApiToken = randomCatApiToken
            
        End Sub
        
        <JsonConstructor>
        Private Sub New (discordToken As String,
                         discordPrefix As String,
                         masterAdminId As ULong,
                         masterGuildId As ULong,
                         botLogChannels As BotLogChannels,
                         googleApiToken As String,
                         randomCatApiToken As String,
                         otherAdmins As List(Of ULong))
            
            Me.DiscordToken = discordToken
            Me.DiscordPrefix = discordPrefix
            Me.MasterAdminId = masterAdminId
            Me.MasterGuildId = masterGuildId
            Me.BotLogChannels = botLogChannels
            Me.GoogleApiToken = googleApiToken
            Me.RandomCatApiToken = randomCatApiToken
            Me.OtherAdmins = otherAdmins
            
        End Sub
        
        Public Shared Async Function LoadAsync() As Task(Of MasterConfig)
            If Not (File.Exists(Filename)) Then Return Await SetupAsync()
            
            Dim json As String = Await File.ReadAllTextAsync(Filename)
            Return JsonConvert.DeserializeObject(Of MasterConfig)(json)
        End Function
        
        Private Shared Async Function SetupAsync() As Task(Of MasterConfig)
            Console.Write("Discord Token: ")
            Dim token As String = Console.ReadLine()

            Console.Write("Prefix [!]: ")
            Dim rawPrefix As String = Console.ReadLine()
            Dim prefix As String = If(String.IsNullOrWhiteSpace(rawPrefix), "!")

            Console.Write("Master Admin ID: ")
            Dim masterAdminId As ULong = ULong.Parse(Console.ReadLine())

            Console.Write("Google API Key: ")
            Dim googleApiKey As String = Console.ReadLine()

            Console.Write("RandomCat API Key: ")
            Dim randomCatApiKey As String = Console.ReadLine()

            Dim config As New MasterConfig(token, prefix, masterAdminId, googleApiKey, randomCatApiKey)
            await config.SaveAsync()
            Return config
        End Function
        
        Public Async Function SaveAsync() As Task
            Await File.WriteAllTextAsync(Filename, JsonConvert.SerializeObject(Me, Formatting.Indented))
        End Function
        
        Public Async Function AssignAsMasterGuildAsync(guildId As ULong) As Task
            _MasterGuildId = guildId
            Await SaveAsync()
        End Function
        
        Public Async Function AssignBotLogChannelAsync(channelId As ULong, botLogType As BotLogType) As Task
            Select botLogType
                Case BotLogType.Common:
                    _BotLogChannels.Common = channelId
                    Exit Select
                Case BotLogType.CommandManager:
                    _BotLogChannels.CommandMngr = channelId
                    Exit Select
                Case BotLogType.GuildManager:
                    _BotLogChannels.GuildMngr = channelId
                    Exit Select
                Case BotLogType.MusicManager:
                    _BotLogChannels.MusicMngr = channelId
                    Exit Select
                Case BotLogType.MusicPlayer:
                    _BotLogChannels.MusicPlayer = channelId
                    Exit Select
                Case BotLogType.AudioChord:
                    _BotLogChannels.AudioChord = channelId
                    Exit Select
                Case BotLogType.TaskScheduler:
                    _BotLogChannels.TaskSch = channelId
                    Exit Select
                Case BotLogType.TimerManager:
                    _BotLogChannels.TimerMngr = channelId
                    Exit Select
            End Select
            
            Await SaveAsync()
        End Function
        
        Public Async Function AssignAdminAsync(userId As ULong) As Task(Of Boolean)
            If _OtherAdmins.Contains(userId) Then Return False
            
            _OtherAdmins.Add(userId)
            Await SaveAsync()
            Return True
        End Function
        
        Public Async Function RemoveAdminAsync(userId As ULong) As Task(Of Boolean)
            If Not (_OtherAdmins.Contains(userId)) Then Return False
            
            _OtherAdmins.Add(userId)
            Await SaveAsync()
            Return True
        End Function
        
    End Class
    
End Namespace
