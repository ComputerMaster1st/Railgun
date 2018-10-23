Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server.Filter

Namespace Commands.Filters
    
    <Group("anticaps"), UserPerms(GuildPermission.ManageMessages), 
        BotPerms(GuildPermission.ManageMessages)>
    Public Class AntiCaps
        Inherits SystemBase
        
        <Command>
        Public Async Function EnableAsync() As Task
            Dim data As FilterCaps = Await Context.Database.FilterCapses.GetOrCreateAsync(Context.Guild.Id)
                
            data.IsEnabled = Not (data.IsEnabled)
                
            Await ReplyAsync($"Anti-Caps is now {Format.Bold(If(data.IsEnabled, "Enabled", "Disabled"))}.")
        End Function
        
        <Command("includebots")>
        Public Async Function IncludeBotsAsync() As Task
            Dim data As FilterCaps = Await Context.Database.FilterCapses.GetOrCreateAsync(Context.Guild.Id)
                
            data.IncludeBots = Not (data.IncludeBots)
            
            Await ReplyAsync($"Anti-Caps is now {Format.Bold(If(data.IncludeBots, "Enabled", "Disabled"))}.")
        End Function
        
        <Command("percent")>
        Public Async Function PercentAsync(percent As Integer) As Task
            If percent < 50 OrElse percent > 100
                await ReplyAsync("Anti-Caps Percentage must be between 50-100.")
                Return
            End If
            
            Dim data As FilterCaps = Await Context.Database.FilterCapses.GetOrCreateAsync(Context.Guild.Id)
                
            data.Percentage = percent
            
            If Not (data.IsEnabled) Then data.IsEnabled = True
            
            await ReplyAsync($"Anti-Caps is now set to trigger at {Format.Bold($"{percent}%")} sensitivity.")
        End Function
        
        <Command("minlength")>
        Public Async Function MinLengthAsync(length As Integer) As Task
            If length < 0
                await ReplyAsync("Please specify a minimum message length of 0 or above.")
                Return
            End If
            
            Dim data As FilterCaps = Await Context.Database.FilterCapses.GetOrCreateAsync(Context.Guild.Id)
            
            data.Length = length
            
            If Not (data.IsEnabled) Then data.IsEnabled = True
            
            Await ReplyAsync($"Anti-Caps is now set to scan messages longer than {Format.Bold(
                length.ToString())} characters.")
        End Function
        
        <Command("ignore")>
        Public Async Function IgnoreAsync(Optional pChannel As ITextChannel = Nothing) As Task
            Dim tc As ITextChannel = If(pChannel, Context.Channel)
            Dim data As FilterCaps = Await Context.Database.FilterCapses.GetOrCreateAsync(Context.Guild.Id)
            
            If data.IgnoredChannels.Where(Function(f) f.ChannelId = tc.Id).Count > 0
                data.IgnoredChannels.RemoveAll(Function(f) f.ChannelId = tc.Id)
                await ReplyAsync("Anti-Caps is now monitoring this channel.")
            Else 
                data.IgnoredChannels.Add(New IgnoredChannels(tc.Id))
                await ReplyAsync("Anti-Caps is no longer monitoring this channel.")
            End If
        End Function
        
        <Command("show")>
        Public Async Function ShowAsync() As Task
            Dim data As FilterCaps = Await Context.Database.FilterCapses.GetAsync(Context.Guild.Id)
            
            If data Is Nothing
                await ReplyAsync("There are no settings available for Anti-Caps. Currently disabled.")
                Return
            End If
            
            Dim output As New StringBuilder
            
            output.AppendLine("Anti-Caps Settings").AppendLine() _
                .AppendFormat("          Status : {0}", If(data.IsEnabled, "Enabled", "Disabled")).AppendLine() _
                .AppendFormat("    Monitor Bots : {0}", If(data.IncludeBots, "Yes", "No")).AppendLine() _
                .AppendFormat("     Sensitivity : {0}", data.Percentage).AppendLine() _
                .AppendFormat("Min. Msg. Length : {0}", data.Length).AppendLine()
            
            If data.IgnoredChannels.Count > 0
                Dim initial As Boolean = True
                Dim deletedChannels As New List(Of IgnoredChannels)
                
                For Each channel As IgnoredChannels In data.IgnoredChannels
                    Dim tc As ITextChannel = Await Context.Guild.GetTextChannelAsync(channel.ChannelId)
                    
                    If tc Is Nothing
                        deletedChannels.Add(channel)
                        Continue For 
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
            Else 
                output.AppendLine("Ignored Channels : None")
            End If
            
            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
        <Command("reset")>
        Public Async Function ResetAsync() As Task
            Dim data As FilterCaps = Await Context.Database.FilterCapses.GetAsync(Context.Guild.Id)
                
            If data Is Nothing
                await ReplyAsync("Anti-Caps has no data to reset.")
                Return
            End If
            
            Context.Database.FilterCapses.Remove(data)
            await ReplyAsync("Anti-Caps has been reset & disabled.")
        End Function

    End Class
    
End NameSpace