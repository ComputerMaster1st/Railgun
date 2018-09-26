Namespace Core.Music.PlayerEventArgs
    
    Public MustInherit Class PlayerEventArgs
        Inherits EventArgs
        
        Public ReadOnly Property GuildId As ULong
        
        Protected Sub New(guildId As ULong)
            Me.GuildId = guildId
        End Sub
    End Class
    
End NameSpace