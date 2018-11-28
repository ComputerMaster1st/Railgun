Imports System.IO
Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Filter

Namespace Commands.Filters
    
    <Group("antiurl"), UserPerms(GuildPermission.ManageMessages), 
        BotPerms(GuildPermission.ManageMessages)>
    Public Class AntiUrl
        Inherits SystemBase
        
        <Command>
        Public Async Function EnableAsync() As Task
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetOrCreateAsync(Context.Guild.Id)
                
            data.IsEnabled = Not (data.IsEnabled)
            
            Await ReplyAsync($"Anti-Url is now {Format.Bold(If(data.IsEnabled, "Enabled", "Disabled"))}.")
        End Function
        
        <Command("includebots")>
        Public Async Function IncludeBotsAsync() As Task
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetOrCreateAsync(Context.Guild.Id)
                
            data.IncludeBots = Not (data.IncludeBots)
            
            Await ReplyAsync($"Anti-Url is now {Format.Bold(If(data.IncludeBots, "Enabled", "Disabled"))}.")
        End Function
        
        <Command("invites")>
        Public Async Function InvitesAsync() As Task
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetOrCreateAsync(Context.Guild.Id)
            
            data.BlockServerInvites = Not (data.BlockServerInvites)
            
            Await ReplyAsync($"Anti-Url is now {Format.Bold(If(data.BlockServerInvites, "Enabled", "Disabled"))}.")
        End Function
        
        <Command("add")>
        Public Async Function AddAsync(url As String) As Task
            Dim newUrl As String = ProcessUrl(url)
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetOrCreateAsync(Context.Guild.Id)
                
            If data.BannedUrls.Contains(newUrl)
                Await ReplyAsync("The Url specified is already listed.")
                Return
            End If
            
            data.BannedUrls.Add(newUrl)
            
            If Not (data.IsEnabled) Then data.IsEnabled = True
            
            await ReplyAsync($"The Url {Format.Bold(newUrl)} is now added to list.")
        End Function
        
        <Command("remove")>
        Public Async Function RemoveAsync(url As String) As Task
            Dim newUrl As String = ProcessUrl(url)
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetAsync(Context.Guild.Id)
            
            If data Is Nothing OrElse Not (data.BannedUrls.Contains(newUrl))
                await ReplyAsync("The Url specified is not listed.")
                Return
            End If
            
            data.BannedUrls.Remove(newUrl)
            
            await ReplyAsync($"The Url {Format.Bold(newUrl)} is now removed from list.")
        End Function
        
        <Command("ignore")>
        Public Async Function IgnoreAsync(Optional pChannel As ITextChannel = Nothing) As Task
            Dim tc As ITextChannel = If(pChannel, Context.Channel)
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetOrCreateAsync(Context.Guild.Id)
            
            If data.IgnoredChannels.Where(Function(f) f.ChannelId = tc.Id).Count > 0
                data.IgnoredChannels.RemoveAll(Function(f) f.ChannelId = tc.Id)
                await ReplyAsync("Anti-Url is now monitoring this channel.")
            Else 
                data.IgnoredChannels.Add(New IgnoredChannels(tc.Id))
                await ReplyAsync("Anti-Url is no longer monitoring this channel.")
            End If
        End Function
        
        <Command("mode")>
        Public Async Function ModeAsync() As Task
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetOrCreateAsync(Context.Guild.Id)
            
            data.DenyMode = Not (data.DenyMode)
            
            If Not (data.IsEnabled) Then data.IsEnabled = True
            
            await ReplyAsync($"Switched Anti-Url Mode to {If(data.DenyMode, Format.Bold("Deny"), 
                Format.Bold("Allow"))}. {If(data.DenyMode, "Deny", "Allow")} all urls except listed.")
        End Function
        
        <Command("show"), BotPerms(ChannelPermission.AttachFiles)>
        Public Async Function ShowAsync() As Task
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync("There are no settings available for Anti-Url. Currently disabled.")
                Return
            End If
            
            Dim output As New StringBuilder
            Dim initial As Boolean = True
            
            output.AppendLine("Anti-Url Settings").AppendLine() _
                .AppendFormat("          Status : {0}", If(data.IsEnabled, "Enabled", "Disabled")).AppendLine() _
                .AppendFormat("            Mode : {0} All", If(data.DenyMode, "Deny", "Allow")).AppendLine() _
                .AppendFormat("   Block Invites : {0}", If(data.BlockServerInvites, "Yes", "No")).AppendLine() _
                .AppendFormat("    Monitor Bots : {0}", If(data.IncludeBots, "Yes", "No")).AppendLine()
            
            If data.IgnoredChannels.Count < 1
                output.AppendLine("Ignored Channels : None")
            Else 
                Dim deletedChannels As New List(Of IgnoredChannels)
                
                For Each channel As IgnoredChannels In data.IgnoredChannels
                    Dim tc As ITextChannel = Await Context.Guild.GetTextChannelAsync(channel.ChannelId)
                    
                    If tc Is Nothing
                        deletedChannels.Add(channel)
                    ElseIf initial
                        output.AppendFormat("Ignored Channels : #{0}", tc.Name).AppendLine()
                        initial = False
                    Else 
                        output.AppendFormat("                 : #{0}", tc.Name).AppendLine()
                    End If
                Next
                
                If deletedChannels.Count > 0
                    For Each channel As IgnoredChannels In deletedChannels
                        data.IgnoredChannels.Remove(channel)
                    Next
                End If
            End If
            
            If data.BannedUrls.Count < 1
                output.AppendFormat("    {0} Urls : None", If(data.DenyMode, "Allowed", "Blocked")).AppendLine()
            Else 
                initial = True
                
                For Each url As String In data.BannedUrls
                    If initial
                        output.AppendFormat("    {0} Urls : {1}", If(data.DenyMode, "Allowed", "Blocked"), url).AppendLine()
                        initial = False
                    Else 
                        output.AppendFormat("                 : {0}", url).AppendLine()
                    End If
                Next
            End If
            
            If output.Length > 1900
                Dim filename As String = $"{Context.Guild.Id}-filter.txt"
                
                await File.WriteAllTextAsync(filename, output.ToString())
                await Context.Channel.SendFileAsync(filename)
                File.Delete(filename)
            Else
                await ReplyAsync(Format.Code(output.ToString()))
            End If
        End Function
        
        <Command("reset")>
        Public Async Function ResetAsync() As Task
            Dim data As FilterUrl = Await Context.Database.FilterUrls.GetAsync(Context.Guild.Id)
                
            If data Is Nothing
                await ReplyAsync("Anti-Url has no data to reset.")
                Return
            End If
            
            Context.Database.FilterUrls.Remove(data)
            
            await ReplyAsync("Anti-Url has been reset & disabled.")
        End Function
        
        Private Function ProcessUrl(url As String) As String
            Dim cleanUrl = url
            Dim parts As String() = { "http://", "https://", "www." }
            
            For Each part As String in parts
                If cleanUrl.Contains(part) Then cleanUrl = cleanUrl.Replace(part, "")
            Next
            
            Return (cleanUrl.Split("/"c, 2))(0)
        End Function
        
    End Class
    
End NameSpace