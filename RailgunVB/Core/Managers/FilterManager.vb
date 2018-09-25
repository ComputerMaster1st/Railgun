Imports Discord
Imports RailgunVB.Core.Filters

Namespace Core.Managers
    
    Public Class FilterManager
    
        Private ReadOnly _filters As New List(Of IMessageFilter)()
        
        Public Sub RegisterFilter(filter As IMessageFilter)
            _filters.Add(filter)
        End Sub
        
        Public Async Function ApplyFilterAsync(msg As IUserMessage) As Task(Of IUserMessage)
            Dim result As IUserMessage = Nothing
            
            For Each filter As IMessageFilter In _filters
                result = Await filter.FilterAsync(msg)
                
                If result IsNot Nothing Then Exit For
            Next
            
            Return result
        End Function
        
    End Class
    
End NameSpace