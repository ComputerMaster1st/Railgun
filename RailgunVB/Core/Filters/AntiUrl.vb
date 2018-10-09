Imports System.Text
Imports System.Text.RegularExpressions
Imports Discord
Imports RailgunVB.Core.Managers
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Filter

Namespace Core.Filters
    
    Public Class AntiUrl
        Implements IMessageFilter
        
        Private ReadOnly _regex As New Regex("(http(s)?)://(www.)?")
        
        Public Sub New(manager As FilterManager)
            manager.RegisterFilter(Me)
        End Sub
        
        Public Async Function FilterAsync(message As IUserMessage, context As TreeDiagramContext) As Task(Of IUserMessage) _
            Implements IMessageFilter.FilterAsync
            If String.IsNullOrWhiteSpace(message.Content) Then Return Nothing
            
            Dim tc As ITextChannel = message.Channel
            Dim data As FilterUrl = Await context.FilterUrls.GetAsync(tc.GuildId)
            
            If data Is Nothing OrElse 
               Not (data.IsEnabled) OrElse 
               (Not (data.IncludeBots) AndAlso (message.Author.IsBot Or message.Author.IsWebhook)) OrElse 
               data.IgnoredChannels.Where(Function(f) f.ChannelId = tc.Id).Count > 0
                Return Nothing
            End If
            
            Dim self As IGuildUser = Await tc.Guild.GetCurrentUserAsync()
            
            If message.Author.Id = self.Id 
                Return Nothing
            ElseIf Not (self.GetPermissions(tc).ManageMessages)
                Await tc.SendMessageAsync(
                    $"{Format.Bold("Anti-Url :")} Triggered but missing {Format.Bold("Manage Messages")} permission!")
                Return Nothing
            End If
            
            Dim user As IGuildUser = Await tc.Guild.GetUserAsync(message.Author.Id)
            Dim content As String = message.Content.ToLower()
            Dim output As New StringBuilder
            
            output.AppendFormat("{0} Deleted {1}'s Message! {2}", Format.Bold("Anti-Url :"), user.Mention, 
                Format.Bold("Reason :"))
            
            If data.BlockServerInvites AndAlso content.Contains("discord.gg/") Then 
                output.AppendFormat("Server Invite")
                Return Await tc.SendMessageAsync(output.ToString())
            ElseIf _regex.IsMatch(content) AndAlso CheckContentForUrl(data, content)
                output.AppendFormat("Unlisted Url Block")
                Return Await tc.SendMessageAsync(output.ToString())
            ElseIf CheckContentForUrl(data, content)
                output.AppendFormat("Listed Url Block")
                Return Await tc.SendMessageAsync(output.ToString())
            End If
            
            Return Nothing
        End Function
        
        Private Function CheckContentForUrl(data As FilterUrl, content As String) As Boolean
            For Each url As String In data.BannedUrls
                If data.DenyMode AndAlso Not (content.Contains(url)) AndAlso _regex.IsMatch(content)
                    Return True
                ElseIf Not (data.DenyMode) AndAlso content.Contains(url)
                    Return True
                End If
            Next
            
            Return False
        End Function
        
    End Class
    
End NameSpace