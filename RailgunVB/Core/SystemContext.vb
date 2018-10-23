Imports Discord
Imports Discord.Commands
Imports TreeDiagram

Namespace Core
    
    Public Class SystemContext
        Inherits CommandContext
        
        Public ReadOnly Property Database As TreeDiagramContext
        
        Public Sub New(client As IDiscordClient, msg As IUserMessage, dbContext As TreeDiagramContext)
            MyBase.New(client, msg)
            Database = dbContext
        End Sub
        
    End Class
    
End NameSpace