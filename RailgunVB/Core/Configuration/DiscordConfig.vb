Imports RailgunVB.Core.Logging

Namespace Core.Configuration
    
    Public Class DiscordConfig
    
        Public ReadOnly Property Token As String
        Public Property Prefix As String
        Public ReadOnly Property MasterAdminId As ULong
        Public ReadOnly Property MasterGuildId As ULong = 0
        Public ReadOnly Property BotLogChannels As BotLogChannels = New BotLogChannels
        Public ReadOnly Property OtherAdmins As List(Of ULong) = New List(Of ULong)()
        
        Public Sub New(token As String, prefix As String, masterAdminId As ULong)
            Me.Token = token
            Me.Prefix = prefix
            Me.MasterAdminId = masterAdminId
        End Sub
        
        Public Sub AssignMasterGuild(guildId As ULong)
            _MasterGuildId = guildId
        End Sub
        
        Public Sub AssignBotLogChannel(channelId As ULong, botLogType As BotLogType)
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
        End Function
        
        Public Function AssignAdmin(userId As ULong) As Boolean
            If _OtherAdmins.Contains(userId) Then Return False
            
            _OtherAdmins.Add(userId)
            Return True
        End Function
        
        Public Function RemoveAdmin(userId As ULong) As Boolean
            If Not (_OtherAdmins.Contains(userId)) Then Return False
            
            _OtherAdmins.Remove(userId)
            Return True
        End Function
        
    End Class
    
End NameSpace