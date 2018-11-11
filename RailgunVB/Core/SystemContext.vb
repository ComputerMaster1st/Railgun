Imports Discord
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection
Imports TreeDiagram

Namespace Core
    
    Public Class SystemContext
        Inherits CommandContext
        
        Private _services As IServiceProvider
        Private _database As TreeDiagramContext = Nothing
        
        Public ReadOnly Property Database As TreeDiagramContext
            Get
                If _database Is Nothing Then _database = _services.GetService(Of TreeDiagramContext) 
                
                Return _database
            End Get
        End Property
        
        Public Sub New(client As IDiscordClient, msg As IUserMessage, services As IServiceProvider)
            MyBase.New(client, msg)
            _services = services
        End Sub
        
        Public Sub DisposeDatabase()
            If _database IsNot Nothing Then _database.Dispose()
        End Sub
        
    End Class
    
End NameSpace