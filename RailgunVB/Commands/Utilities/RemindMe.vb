Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Managers
Imports TreeDiagram
Imports TreeDiagram.Models.TreeTimer

Namespace Commands.Utilities
    
    <Group("remindme")>
    Public Class RemindMe
        Inherits SystemBase
        
        Private ReadOnly _timerManager As TimerManager

        Public Sub New(timerManager As TimerManager)
            _timerManager = timerManager
        End Sub
        
        <Command>
        Public Async Function RemindMeAsync(expireIn As String, <Remainder> message As String) As Task
            If String.IsNullOrWhiteSpace(message)
                Await ReplyAsync("You didn't specify a message to remind you about.")
                Return
            ElseIf message.Length > 1800
                Await ReplyAsync("Remind me has a character limit of 1800. Please shorten the message.")
                Return
            End If
            
            Dim times As String() = expireIn.Split(":"c)
            Dim invalidFormat As New StringBuilder
            
            invalidFormat.AppendFormat("The time format given is {0}! Please use one of the following formats...", 
                                       Format.Bold("INVALID")).AppendLine() _
                .AppendLine() _
                .AppendFormat("{0} << Minutes || 10 Minutes Example : {1}", Format.Code("m"), 
                              Format.Code("10 this is my msg.")).AppendLine() _
                .AppendFormat("{0} << Hours:Minutes || 1 Hour, 10 Minutes Example : {1}", Format.Code("h:m"), 
                              Format.Code("1:10 this is my msg.")).AppendLine() _
                .AppendFormat("{0} << Days:Hours:Minutes || 1 Day, 1 Hour, 10 Minutes Example : {1}", 
                              Format.Code("d:h:m"), Format.Code("1:1:10 this is my msg."))
            
            If times.Length > 3
                Await ReplyAsync(invalidFormat.ToString())
                Return
            End If
            
            Dim dhm As Integer() = { 0, 0, 0 }
            Dim i = dhm.Length - times.Length
            Dim ti = 0
            Dim number = 0
            
            While i < dhm.Length
                If Not (Integer.TryParse(times(ti), number))
                    await ReplyAsync(invalidFormat.ToString())
                    Return
                End If
                
                dhm(i) = number
                    
                i += 1
                ti += 1
            End While
            
            Dim expireTime As DateTime = DateTime.UtcNow.AddDays(dhm(0)).AddHours(dhm(1)).AddMinutes(dhm(2))
            
            If expireTime < DateTime.UtcNow
                Dim output As New StringBuilder
                
                output.AppendFormat("{0}, you asked me to {1} remind you about the following...", 
                                    Context.User.Mention, Format.Bold("INSTANTLY")).AppendLine() _
                    .AppendLine() _
                    .AppendLine(message)
                
                Await ReplyAsync(output.ToString())
                Return
            End If
            
            Dim data As TimerRemindMe = Await Context.Database.TimerRemindMes.CreateAsync()
            
            data.GuildId = Context.Guild.Id
            data.TextChannelId = Context.Channel.Id
            data.UserId = Context.User.Id
            data.Message = message
            data.TimerExpire = expireTime
            
            Await _timerManager.CreateAndStartRemindMeContainer(data, True)
            Await ReplyAsync($"Reminder has been created! You'll be pinged here at {Format.Bold(
                data.TimerExpire.ToString())} UTC.")
        End Function
        
    End Class
    
End NameSpace