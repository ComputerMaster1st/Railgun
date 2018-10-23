Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports TreeDiagram
Imports TreeDiagram.Models.User

Namespace Commands.User
    
    <Group("myself"), [Alias]("self")>
    Public Class Myself
        Inherits SystemBase
        
        <Command("mention")>
        Public Async Function MentionsAsync() As Task
            Dim data As UserMention = Await Context.Database.UserMentions.GetOrCreateAsync(Context.User.Id)
            
            If data.DisableMentions
                Context.Database.UserMentions.Remove(data)
                await ReplyAsync($"Personal mentions are now {Format.Bold("Enabled")}.")
                Return
            End If
            
            data.DisableMentions = Not (data.DisableMentions)
            
            await ReplyAsync($"Personal mentions are now {Format.Bold("Disabled")}.")
        End Function
        
        <Command("prefix")>
        Public Async Function PrefixAsync(<Remainder> Optional input As String = Nothing) As Task
            Dim data As UserCommand = Await Context.Database.UserCommands.GetAsync(Context.User.Id)
            
            If String.IsNullOrWhiteSpace(input) AndAlso data Is Nothing
                await ReplyAsync("No prefix has been specified. Please specify a prefix.")
                Return
            ElseIf String.IsNullOrWhiteSpace(input) AndAlso data IsNot Nothing
                Context.Database.UserCommands.Remove(data)
                await ReplyAsync("Personal prefix has been removed.")
                Return
            End If
            
            data = Await Context.Database.UserCommands.GetOrCreateAsync(Context.User.Id)
            data.Prefix = input
            
            await ReplyAsync($"Personal prefix has been set! `{input} <command>`!")
        End Function
        
        <Command("show")>
        Public Async Function SHowAsync() As Task
            Dim prefix As UserCommand = Await Context.Database.UserCommands.GetAsync(Context.User.Id)
            Dim mention As UserMention = Await Context.Database.UserMentions.GetAsync(Context.User.Id)
            Dim output As New StringBuilder
            
            output.AppendLine("Railgun User Configuration:").AppendLine() _ 
                .AppendFormat("       Username : {0}#{1}", Context.User.Username, 
                              Context.User.DiscriminatorValue).AppendLine() _
                .AppendFormat("        User ID : {0}", Context.User.Id).AppendLine().AppendLine() _
                .AppendFormat("  Allow Mention : {0}", If(mention IsNot Nothing, "No", "Yes")).AppendLine() _
                .AppendFormat("Personal Prefix : {0}", If(prefix IsNot Nothing, prefix.Prefix, "Not Set")).AppendLine()
            
            await ReplyAsync(Format.Code(output.ToString()))
        End Function
        
    End Class
    
End NameSpace