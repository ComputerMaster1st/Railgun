Imports Discord

Namespace Core.Music
    
    Public Class PlayerContainer
        
        Public ReadOnly Property GuildId As ULong
        Public ReadOnly Property TextChannel As ITextChannel
        Public ReadOnly Property Player As Player

        Public Sub New(tc As ITextChannel, player As Player)
            _GuildId = tc.GuildId
            _TextChannel = tc
            _Player = player
        End Sub
        
    End Class
    
End NameSpace