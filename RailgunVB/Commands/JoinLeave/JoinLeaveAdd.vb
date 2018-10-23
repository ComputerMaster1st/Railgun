Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports TreeDiagram
Imports TreeDiagram.Enums
Imports TreeDiagram.Models.Server

Namespace Commands.JoinLeave
    
    Partial Public Class JoinLeave
    
        <Group("add")>
        Public Class JoinLeaveAdd
            Inherits SystemBase
            
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
                
                Dim data As ServerJoinLeave = Await Context.Database.ServerJoinLeaves.GetOrCreateAsync(Context.Guild.Id)
                
                If (type = MsgType.Join AndAlso data.JoinMessages.Contains(msg)) OrElse 
                   (type = MsgType.Leave AndAlso data.LeaveMessages.Contains(msg))
                    Await ReplyAsync("Specified message is already listed.")
                    Return
                End If
                
                data.AddMessage(msg, type)
                
                Await ReplyAsync($"Successfully added {Format.Code(msg)} to {Format.Bold(type.ToString())} message.")
            End Function
            
        End Class
        
    End Class
    
End NameSpace