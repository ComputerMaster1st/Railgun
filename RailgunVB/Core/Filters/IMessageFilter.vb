Imports Discord
Imports TreeDiagram

Namespace Core.Filters
    
    Public Interface IMessageFilter
    
        Function FilterAsync(message As IUserMessage, context As TreeDiagramContext) As Task(Of IUserMessage)
        
    End Interface
    
End NameSpace