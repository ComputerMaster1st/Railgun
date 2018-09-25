Imports Discord

Namespace Core.Filters
    
    Public Interface IMessageFilter
    
        Function FilterAsync(message As IUserMessage) As Task(Of IUserMessage)
        
    End Interface
    
End NameSpace