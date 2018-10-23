Imports Discord
Imports Discord.Commands
Imports TreeDiagram
Imports TreeDiagram.Enums
Imports TreeDiagram.Models.Server

Namespace Commands.JoinLeave
    
    Partial Public Class JoinLeave
    
        <Group("add")>
        Public Class JoinLeaveAdd
            Inherits ModuleBase
            
            Private ReadOnly _dbContext As TreeDiagramContext
    
            Public Sub New(dbContext As TreeDiagramContext)
                _dbContext = dbContext
            End Sub
            
            <Command("joinmsg")>
            Public Async Function JoinAsync(<Remainder> msg As String) As Task
                Await MsgHandlerAsync(msg, MsgType.Join)
            End Function
            
            <Command("leavemsg")>
            Public Async Function LeaveAsync(<Remainder> msg As String) As Task
                Await MsgHandlerAsync(msg, MsgType.Leave)
            End Function
            
            Private Async Function MsgHandlerAsync(msg As String, type As MsgType) As Task
                If String.IsNullOrWhiteSpace(msg)
                    await ReplyAsync("Please specify a message to add.")
                    Return
                End If
                
                Dim data As ServerJoinLeave = Await _dbContext.ServerJoinLeaves.GetOrCreateAsync(Context.Guild.Id)
                
                If (type = MsgType.Join AndAlso data.JoinMessages.Contains(msg)) OrElse 
                   (type = MsgType.Leave AndAlso data.LeaveMessages.Contains(msg))
                    await ReplyAsync("Specified message is already listed.")
                    Return
                End If
                
                data.AddMessage(msg, type)
                
                Await _dbContext.SaveChangesAsync()
                await ReplyAsync($"Successfully added {Format.Code(msg)} to {Format.Bold(type.ToString())} message.")
            End Function
        End Class
        
    End Class
    
End NameSpace