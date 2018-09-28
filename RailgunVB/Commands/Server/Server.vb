Imports System.IO
Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Logging
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Warning

Namespace Commands.Server
    
    <Group("server")>
    Partial Public Class Server
        Inherits ModuleBase
        
        Private ReadOnly _log As Log
        Private ReadOnly _dbContext As TreeDiagramContext

        Public Sub New(log As Log, dbContext As TreeDiagramContext)
            _log = log
            _dbContext = dbContext
        End Sub
        
        <Command("leave"), UserPerms(GuildPermission.ManageGuild)>
        Public Async Function LeaveAsync() As Task
            await ReplyAsync("My presence is no longer required. Goodbye everyone!")
            await Task.Delay(500)
            await Context.Guild.LeaveAsync()
        End Function
        
        <Command("clear"), UserPerms(GuildPermission.ManageMessages), 
            BotPerms(GuildPermission.ManageMessages And GuildPermission.ReadMessageHistory)>
        Public Async Function ClearAsync(Optional count As Integer = 100) As Task
            Dim deleted = 0
            
            While count > 0
                Dim subCount As Integer = If(count >= 100, 100, count)
                Dim tc As ITextChannel = Context.Channel
                Dim msgs = Await tc.GetMessagesAsync(subCount).FlattenAsync()
                Dim msgsToDelete As List(Of IMessage) = (From msg In msgs Where msg.CreatedAt > DateTime.Now.AddDays(- 13).AddHours(- 23).AddMinutes(- 50)).ToList()

                Await tc.DeleteMessagesAsync(msgsToDelete)
                
                deleted += msgsToDelete.Count
                
                If msgsToDelete.Count = subCount
                    count -= subCount
                Else 
                    Exit While
                End if
            End While
            
            await ReplyAsync($"Up to {Format.Bold(deleted.ToString())} messages have been deleted from the channel.")
        End Function
        
        <Command("id"), UserPerms(GuildPermission.ManageGuild)>
        Public Async Function IdAsync() As Task
            await ReplyAsync($"This server's ID is {Format.Bold(Context.Guild.Id.ToString())}")
        End Function
        
        <Command("kick"), UserPerms(GuildPermission.KickMembers), BotPerms(GuildPermission.KickMembers)>
        Public Async Function KickAsync(user As IUser, 
                                        <Remainder> Optional reason As String = "No Reason Specified") As Task
            Dim gUser As IGuildUser = Await Context.Guild.GetUserAsync(user.Id)
            
            Await gUser.KickAsync(reason)
            await ReplyAsync(
                $"{Format.Bold(user.Username)} has been kicked from the server. Reason: {Format.Bold(reason)}")
            
            Dim output As New StringBuilder
            
            output.AppendFormat("User Kicked || <{0} ({1})> {2}#{3}", Context.Guild.Name, Context.Guild.Id, user.Username, user.DiscriminatorValue).AppendLine().AppendFormat("---- Reason : {0}", reason)
            
            await _log.LogToBotLogAsync(output.ToString(), BotLogType.Common)
        End Function
        
        <Command("ban"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)>
        Public Async Function BanAsync(user As IUser, 
                                       <Remainder> Optional reason As String = "No Reason Specified") As Task
            Dim data As ServerWarning = Await _dbContext.ServerWarnings.GetAsync(Context.Guild.Id)
            
            Await Context.Guild.AddBanAsync(user, 7, reason)
            
            If data IsNot Nothing AndAlso data.Warnings.Count(Function (find) find.UserId = user.Id) > 0
                data.ResetWarnings(user.Id)
                Await _dbContext.SaveChangesAsync()
            End If
            
            await ReplyAsync(
                $"{Format.Bold(user.Username)} has been banned from the server. Reason: {Format.Bold(reason)}")
            
            Dim output As New StringBuilder
            
            output.AppendFormat("User Banned || <{0} ({1})> {2}#{3}", Context.Guild.Name, Context.Guild.Id, user.Username, user.DiscriminatorValue).AppendLine().AppendFormat("---- Reason : {0}", reason)
            
            await _log.LogToBotLogAsync(output.ToString(), BotLogType.Common)
        End Function
        
        <Command("unban"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers)>
        Public Async Function UnbanAsync(user As ULong) As Task
            Await Context.Guild.RemoveBanAsync(user)
            await ReplyAsync($"User ID {Format.Bold(user.ToString())} is now unbanned from the server.")
            await _log.LogToBotLogAsync(
                $"User Unbanned || <{Context.Guild.Name} ({Context.Guild.Id})> ID : {user}", BotLogType.Common)
        End Function
        
        <Command("banlist"), UserPerms(GuildPermission.BanMembers), BotPerms(GuildPermission.BanMembers),
            BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function BanlistAsync() As Task
            Dim bans = Await Context.Guild.GetBansAsync()
            Dim output As New StringBuilder
            
            output.AppendLine("Guild Banned Users List:").AppendLine()
            
            If bans.Count > 0
                For Each ban As IBan In bans
                    output.AppendFormat("{0} ({1}) => [{2}]", ban.User.Username, ban.User.Id, ban.Reason).AppendLine()
                Next
            Else
                output.AppendLine("Empty.")
            End If
            
            output.AppendLine().AppendLine("End Of Banned User List!")
            
            If output.Length < 1950
                await ReplyAsync(output.ToString())
                Return
            End If
            
            Dim filename As String = $"{Context.Guild.Name}-Banlist.txt"
            await File.WriteAllTextAsync(filename, output.ToString())
            await Context.Channel.SendFileAsync(filename, "Ban List")
            File.Delete(filename)
        End Function
        
    End Class
    
End NameSpace