Imports Discord
Imports RailgunVB.Core.Managers
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Filter

Namespace Core.Filters
    
    Public Class AntiCaps
        Implements IMessageFilter
        
        Public Sub New(manager As FilterManager)
            manager.RegisterFilter(Me)
        End Sub
        
        Public Async Function FilterAsync(message As IUserMessage, context As TreeDiagramContext) As Task(
            Of IUserMessage) Implements IMessageFilter.FilterAsync
            If String.IsNullOrWhiteSpace(message.Content) Then Return Nothing
            
            Dim tc As ITextChannel = message.Channel
            Dim data As FilterCaps = Await context.FilterCapses.GetAsync(tc.GuildId)
            
            If data Is Nothing OrElse Not (data.IsEnabled) OrElse 
               (Not (data.IncludeBots) AndAlso (message.Author.IsBot Or message.Author.IsWebhook)) OrElse 
               data.IgnoredChannels.Where(Function(f) f.ChannelId = tc.Id).Count > 0 OrElse 
               message.Content.Length < data.Length
                Return Nothing
            End If
            
            Dim self As IGuildUser = Await tc.Guild.GetCurrentUserAsync()
            
            If message.Author.Id = self.Id
                Return Nothing
            ElseIf Not (self.GetPermissions(tc).ManageMessages)
                Await tc.SendMessageAsync(
                    $"{Format.Bold("Anti-Caps : ")} Triggered but missing {Format.Bold("Manage Messages")} permission!")
                Return Nothing
            End If
            
            Dim user As IGuildUser = Await tc.Guild.GetUserAsync(message.Author.Id)
            Dim characterCount As Double = 0
            Dim capsCount As Double = 0
            
            For Each c As Char in message.Content
                If Char.IsLetter(c)
                    characterCount += 1
                    If Char.IsUpper(c) Then capsCount += 1
                End If
            Next
            
            If capsCount < 1 OrElse characterCount < data.Length Then Return Nothing
            
            Dim percentage As Double = (capsCount / characterCount) * 100
            
            If percentage < data.Percentage Then Return Nothing
            
            Return Await tc.SendMessageAsync($"{Format.Bold("Anti-Caps :")} Deleted {user.Mention}'s Message! ({ _ 
                                                Format.Bold(Math.Round(percentage).ToString() + "%")} Caps)")
        End Function
        
    End Class
    
End NameSpace