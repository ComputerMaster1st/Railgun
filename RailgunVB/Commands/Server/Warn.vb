Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Warning

Namespace Commands.Server
    
    <Group("warn")>
    Public Class Warn
        Inherits SystemBase
    
        Private ReadOnly _log As Log

        Public Sub New(log As Log)
            _log = log
        End Sub
        
        Private Async Function WarnUserAsync(data As ServerWarning, user As IUser, reason As String) As Task
            data.AddWarning(user.Id, reason)
            await ReplyAsync($"{user.Mention} has received a warning! Reason: {Format.Bold(reason)}")
        End Function
        
        <Command, UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)>
        Public Async Function WarnAsync(user As IUser, 
                                        <Remainder> Optional reason As String = "No Reason Specified") As Task
            If user.Id = Context.Client.CurrentUser.Id
                await ReplyAsync("You can not warn me. Just No. Baka.")
                Return
            End If
            
            Dim data As ServerWarning = Await Context.Database.ServerWarnings.GetOrCreateAsync(Context.Guild.Id)
            Dim userWarnings As List(Of String) = data.GetWarnings(user.Id)
            
            If data.WarnLimit < 1
                await ReplyAsync("User Warnings are currently disabled. You can enable it by changing the warning limit.")
                Return
            ElseIf userWarnings Is Nothing OrElse userWarnings.Count < data.WarnLimit
                await WarnUserAsync(data, user, reason)
                Return
            End If
            
            Try
                await Context.Guild.AddBanAsync(user, 7, reason)
                
                data.ResetWarnings(user.Id)
                
                await ReplyAsync($"{Format.Bold(user.Username)} has been Auto-Banned from the server. Reason: {Format.Bold($"{reason} & Too many warnings!")}")
                await _log.LogToBotLogAsync($"Auto Ban || <{Context.Guild.Name} ({Context.Guild.Id})> Successful! {user.Username}#{user.DiscriminatorValue}", BotLogType.Common)
            Catch e As Exception
                ReplyAsync("Unable to auto-ban user! Please be sure that I'm higher up on the role list.").GetAwaiter()
                
                Dim output As New StringBuilder
                
                output.AppendFormat("Auto Ban || <{0} ({1})> Failure!", Context.Guild.Name, 
                                    Context.Guild.Id).AppendLine() _ 
                    .AppendFormat("---- Reason : {0}", e.Message)
                _log.LogToBotLogAsync(output.ToString(), BotLogType.Common).GetAwaiter()
            End Try
        End Function
        
        <Command("list"), UserPerms(GuildPermission.BanMembers)>
        Public Async Function ListAsync() As Task
            Dim data As ServerWarning = Await Context.Database.ServerWarnings.GetAsync(Context.Guild.Id)
            
            If data is Nothing OrElse data.Warnings.Count < 1
                await ReplyAsync("There are currently no users with warnings.")
                Return
            End If
            
            Dim unknownUsers As New List(Of ULong)
            Dim output As New StringBuilder
            
            output.AppendFormat("There are currently {0} user(s) with warnings for...", 
                                Format.Bold(data.Warnings.Count.ToString())).AppendLine().AppendLine()
            
            For Each warning In data.Warnings
                Dim user As IGuildUser = Await Context.Guild.GetUserAsync(warning.UserId)
                
                If user Is Nothing
                    unknownUsers.Add(warning.UserId)
                    Continue For
                End If
                
                output.AppendFormat("---- {0} ({1})", Format.Bold($"{user.Username}#{user.DiscriminatorValue}"), 
                                    warning.Reasons.Count).AppendLine()
                
                For Each reason As String In warning.Reasons
                    output.AppendFormat("    ---- {0}", Format.Bold(reason)).AppendLine()
                Next
                
                output.AppendLine()
            Next
            
            If unknownUsers.Count > 0
                For Each user As ULong In unknownUsers
                    data.ResetWarnings(user)
                Next
                
                output.AppendFormat("Detected {0} unknown user(s)! These user(s) have been automatically removed from the list.", UnknownUsers.Count).AppendLine()
            End If
            
            await ReplyAsync(output.ToString())
        End Function
        
        <Command("mylist")>
        Public Async Function MyListAsync() As Task
            Dim data As ServerWarning = Await Context.Database.ServerWarnings.GetAsync(Context.Guild.Id)
            
            If data is Nothing OrElse data.Warnings.Count < 1
                await ReplyAsync("There are currently no users with warnings.")
                Return
            End If
            
            Dim warnings As List(Of String) = data.GetWarnings(Context.User.Id)
            
            If warnings Is Nothing OrElse warnings.Count < 1
                await ReplyAsync("You have no warnings to your name.")
                Return
            End If
            
            Dim output As New StringBuilder
            
            output.AppendFormat("You have been warned {0} time(s) for...", 
                                Format.Bold(warnings.Count.ToString())).AppendLine().AppendLine()
            
            For Each reason As String In warnings
                output.AppendFormat("---- {0}", Format.Bold(reason)).AppendLine()
            Next
            
            await ReplyAsync(output.ToString())
        End Function
        
        <Command("clear"), UserPerms(GuildPermission.BanMembers)>
        Public Async Function ClearAsync(user As IUser) As Task
            Dim data As ServerWarning = Await Context.Database.ServerWarnings.GetAsync(Context.Guild.Id)
            
            If data Is Nothing OrElse data.Warnings.Count < 1
                await ReplyAsync($"There are no warnings currently issued to {user.Mention}.")
                Return
            End If
            
            Dim warnings As List(Of String) = data.GetWarnings(user.Id)
            
            If warnings Is Nothing OrElse warnings.Count < 1
                await ReplyAsync($"There are no warnings currently issued to {user.Mention}.")
                Return
            End If
            
            data.ResetWarnings(user.Id)
            
            await ReplyAsync($"{user.Mention} no longer has any warnings.")
        End Function
        
        <Command("empty"), UserPerms(GuildPermission.ManageGuild)>
        Public Async Function EmptyAsync() As Task
            Dim data As ServerWarning = Await Context.Database.ServerWarnings.GetAsync(Context.Guild.Id)
            
            If data Is Nothing OrElse data.Warnings.Count < 1
                await ReplyAsync("Warnings list is already empty.")
                Return
            End If
            
            data.Warnings.Clear()
            
            await ReplyAsync("Warnings list is now empty.")
        End Function
        
        <Command("limit"), UserPerms(GuildPermission.ManageGuild)>
        Public Async Function WarnLimitAsync(Optional limit As Integer = 5) As Task
            Dim data As ServerWarning = Await Context.Database.ServerWarnings.GetOrCreateAsync(Context.Guild.Id)
            
            If limit < 0
                await ReplyAsync("The limit entered is invalid. Must be 0 or higher.")
                Return
            End If
            
            Dim message As String
            
            If limit > 0
                message = $"Auto-Ban{If(data.WarnLimit = 0, " is now enabled and the", "")} warning limit is now set to {Format.Bold(limit.ToString())}. You can disable warnings by changing the limit to 0."
                data.WarnLimit = limit
            Else 
                data.WarnLimit = limit
                message = "Auto-Ban has been disabled."
            End If
            
            await ReplyAsync(message)
        End Function
        
        <Command("reset"), UserPerms(GuildPermission.ManageGuild)>
        Public Async Function ResetAsync() As Task
            Dim data As ServerWarning = Await Context.Database.ServerWarnings.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync("Warnings has no data to reset.")
                Return
            End If
            
            Context.Database.ServerWarnings.Remove(data)
            
            await ReplyAsync("Warnings has been reset & disabled.")
        End Function
        
    End Class
    
End NameSpace