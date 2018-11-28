Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.JoinLeave
    
    <Group("joinleave"), UserPerms(GuildPermission.ManageGuild)>
    Partial Public Class JoinLeave
        Inherits SystemBase
        
        <Command>
        Public Async Function EnableAsync() As Task
            Dim data As ServerJoinLeave = Await Context.Database.ServerJoinLeaves.GetOrCreateAsync(Context.Guild.Id)
            
            If data.ChannelId = Context.Channel.Id
                data.ChannelId = 0
                
                Await ReplyAsync($"Join/Leave Notifications is now {Format.Bold("Disabled")}.")
                Return
            End If
            
            data.ChannelId = Context.Channel.Id
            
            Await ReplyAsync($"Join/Leave Notifications is now {Format.Bold(If(data.ChannelId = 0, "Enabled & Set", 
                                                                               "Set"))} to this channel.")
        End Function
        
        <Command("deleteafter"), BotPerms(ChannelPermission.ManageMessages)>
        Public Async Function DeleteAfterAsync(Optional minutes As Integer = 0) As Task
            If minutes < 0
                Await ReplyAsync("Minutes can not be less than 0.")
                Return
            End If
            
            Dim data As ServerJoinLeave = Await Context.Database.ServerJoinLeaves.GetOrCreateAsync(Context.Guild.Id)
            
            If minutes = 0 AndAlso data.DeleteAfterMinutes = 0
                Await ReplyAsync("Already set to not delete Join/Leave notifications.")
                Return
            ElseIf minutes = 0 AndAlso data.DeleteAfterMinutes <> 0
                data.DeleteAfterMinutes = 0
                Await ReplyAsync("No longer deleting Join/Leave notifications.")
                Return
            End If
            
            data.DeleteAfterMinutes = minutes
            
            Await ReplyAsync($"Join/Leave notifications will now be deleted after {minutes} minutes.")
        End Function
        
        <Command("sendtodm")>
        Public Async Function DmAsync() As Task
            Dim data As ServerJoinLeave = Await Context.Database.ServerJoinLeaves.GetAsync(Context.Guild.Id)
            
            If data Is Nothing OrElse data.ChannelId = 0
                Await ReplyAsync("Join/Leave Notifications is currently turned off. Please turn on before using this command.")
                Return
            End If
            
            data.SendToDM = Not (data.SendToDM)
            
            Await ReplyAsync($"Join/Leave Notification will {Format.Bold(If(data.SendToDM, "Now", "No Longer")) _ 
                                } be sent via DMs.")
        End Function
        
        <Command("show"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function ShowAsync() As Task
            Dim data As ServerJoinLeave = Await Context.Database.ServerJoinLeaves.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync("Join/Leave Notifications has not been configured!")
                Return
            End If
            
            Dim output As New StringBuilder
            Dim tc As ITextChannel = If(data.ChannelId <> 0, await Context.Guild.GetTextChannelAsync(data.ChannelId), 
                                        Nothing)
            Dim tempString As String
            
            If data.SendToDM
                tempString = "Private Messages"
            ElseIf tc IsNot Nothing
                tempString = tc.Name
            Else 
                tempString = "TextChannel not set or missing?"
            End If
            
            output.AppendLine(Format.Bold("Join/Leave Notifications")).AppendLine() _
                .AppendFormat("Send To : {0}", Format.Bold(tempString)).AppendLine().AppendLine() _ 
                .AppendLine(Format.Bold("Join Messages :")).AppendLine()
            
            For Each msg As String in data.JoinMessages
                output.AppendFormat("[{0}] : {1}", Format.Code(data.JoinMessages.IndexOf(msg).ToString()), 
                                    msg).AppendLine()
            Next
            
            output.AppendLine().AppendLine(Format.Bold("Leave Messages :")).AppendLine()
            
            For Each msg As String in data.LeaveMessages
                output.AppendFormat("[{0}] : {1}", Format.Code(data.LeaveMessages.IndexOf(msg).ToString()), 
                                    msg).AppendLine()
            Next
            
            If output.Length <= 1950
                await ReplyAsync(output.ToString())
                Return
            End If
            
            Await SendStringAsFileAsync(Context.Channel, "JoinLeave Notifications.txt", output.ToString())
        End Function
        
        <Command("reset")>
        Public Async Function ResetAsync() As Task
            Dim data As ServerJoinLeave = Await Context.Database.ServerJoinLeaves.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                Await ReplyAsync("Join/Leave Notifications has no data to reset.")
                Return
            End If
            
            Context.Database.ServerJoinLeaves.Remove(data)
            
            Await ReplyAsync("Join/Leave Notifications has been reset & disabled.")
        End Function
        
    End Class
    
End NameSpace