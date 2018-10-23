Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports TreeDiagram
Imports TreeDiagram.Enums
Imports TreeDiagram.Models.Server

Namespace Commands.JoinLeave
    
    Partial Public Class JoinLeave
    
        <Group("remove")>
        Public Class JoinLeaveRemove
            Inherits SystemBase
            
            <Command("joinmsg")>
            Public Async Function JoinAsync(msg As Integer) As Task
                Await MsgHandlerAsync(msg, MsgType.Join)
            End Function
            
            <Command("leavemsg")>
            Public Async Function LeaveAsync(msg As Integer) As Task
                Await MsgHandlerAsync(msg, MsgType.Leave)
            End Function
            
            Private Async Function MsgHandlerAsync(index As Integer, type As MsgType) As Task
                If index < 0
                    await ReplyAsync("The specified Id can not be lower than 0.")
                    Return
                End If
                
                Dim data As ServerJoinLeave = Await Context.Database.ServerJoinLeaves.GetAsync(Context.Guild.Id)
                
                If data Is Nothing
                    Await ReplyAsync("Join/Leave has yet to be configured.")
                    Return
                ElseIf (type = MsgType.Join AndAlso data.JoinMessages.Count <= index) OrElse 
                       (type = MsgType.Leave AndAlso data.LeaveMessages.Count <= index)
                    Await ReplyAsync("Specified message is not listed.")
                    Return
                End If
                
                data.RemoveMessage(index, type)
                
                Await ReplyAsync($"Successfully removed from {Format.Bold(type.ToString())} messages.")
            End Function
            
        End Class
        
    End Class
    
End NameSpace