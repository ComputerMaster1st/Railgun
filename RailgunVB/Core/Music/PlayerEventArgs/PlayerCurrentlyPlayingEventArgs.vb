Imports AudioChord

Namespace Core.Music.PlayerEventArgs
    
    Public Class PlayerCurrentlyPlayingEventArgs
        Inherits PlayerEventArgs
        
        Public ReadOnly Property SongId As String
        Public ReadOnly Property Metadata As SongMetadata
        
        Public Sub New(guildId As ULong, songId As String, metaData As SongMetadata)
            MyBase.New(guildId)
            Me.SongId = songId
            Me.Metadata = metaData
        End Sub
        
    End Class
    
End NameSpace