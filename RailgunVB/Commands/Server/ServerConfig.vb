Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Server

    Partial Public Class Server
    
        <Group("config"), UserPerms(GuildPermission.ManageGuild)>
        Public Class ServerConfig
            Inherits ModuleBase
        
            Private ReadOnly _dbContext As TreeDiagramContext

            Public Sub New(dbContext As TreeDiagramContext)
                _dbContext = dbContext
            End Sub
            
            Protected Overrides Async Sub AfterExecute(command As CommandInfo)
                Await _dbContext.SaveChangesAsync()
                MyBase.AfterExecute(command)
            End Sub
        
            <Command("mention")>
            Public Async Function MentionAsync() As Task
                Dim data As ServerMention = Await _dbContext.ServerMentions.GetAsync(Context.Guild.Id)
            
                If data IsNot Nothing
                    _dbContext.ServerMentions.Remove(data)
                    await ReplyAsync($"Server mentions are now {Format.Bold("Enabled")}.")
                    Return
                End If
            
                data = Await _dbContext.ServerMentions.GetOrCreateAsync(Context.Guild.Id)
                data.DisableMentions = True
            
                await ReplyAsync($"Server mentions are now {Format.Bold("Disabled")}.")
            End Function
        
            <Command("prefix")>
            Public Async Function PrefixAsync(<Remainder> Optional input As String = Nothing) As Task
                Dim data As ServerCommand = Await _dbContext.ServerCommands.GetOrCreateAsync(Context.Guild.Id)
            
                If String.IsNullOrWhiteSpace(input) AndAlso String.IsNullOrEmpty(data.Prefix)
                    await ReplyAsync("No prefix has been specified. Please specify a prefix.")
                    Return
                ElseIf String.IsNullOrWhiteSpace(input) AndAlso Not (String.IsNullOrEmpty(data.Prefix))
                    data.Prefix = String.Empty
                    await ReplyAsync("Server prefix has been removed.")
                    Return
                End If
            
                data.Prefix = input
            
                await ReplyAsync($"Server prefix has been set! `{input} <command>`!")
            End Function
        
            <Command("deletecmd"), BotPerms(GuildPermission.ManageMessages)>
            Public Async Function DeleteCmdAsync() As Task
                Dim data As ServerCommand = Await _dbContext.ServerCommands.GetOrCreateAsync(Context.Guild.Id)
            
                data.DeleteCmdAfterUse = Not (data.DeleteCmdAfterUse)
            
                await ReplyAsync(
                    $"Commands used will {Format.Bold(If(data.DeleteCmdAfterUse, "now", "no longer"))} be deleted.")
            End Function
        
            <Command("respondtobots")>
            Public Async Function RespondAsync() As Task
                Dim data As ServerCommand = Await _dbContext.ServerCommands.GetOrCreateAsync(Context.Guild.Id)
            
                data.RespondToBots = Not (data.RespondToBots)
            
                await ReplyAsync(
                    $"I will {Format.Bold(If(data.RespondToBots, "now", "no longer"))} respond to other bots.")
            End Function
            
            <Command("show")>
            Public Async Function ShowAsync() As Task
                Dim command As ServerCommand = Await _dbContext.ServerCommands.GetAsync(Context.Guild.Id)
                Dim mention As ServerMention = Await _dbContext.ServerMentions.GetAsync(Context.Guild.Id)
                Dim output As New StringBuilder
                
                output.AppendLine("Railgun Server Configuration").AppendLine() _
                    .AppendFormat("    Server Name : {0}", Context.Guild.Name).AppendLine() _
                    .AppendFormat("      Server ID : {0}", Context.Guild.Id).AppendLine().AppendLine() _
                    .AppendFormat("     Delete CMD : {0}", 
                        If(command IsNot Nothing AndAlso command.DeleteCmdAfterUse, "Yes", "No")).AppendLine() _
                    .AppendFormat("Respond To Bots : {0}",
                        If(command IsNot Nothing AndAlso command.RespondToBots, "Yes", "No")).AppendLine() _
                    .AppendFormat("  Allow Mention : {0}", 
                        If(mention IsNot Nothing AndAlso mention.DisableMentions, "No", "Yes")).AppendLine() _
                    .AppendFormat("  Server Prefix : {0}", 
                        If(command IsNot Nothing AndAlso Not (String.IsNullOrEmpty(command.Prefix)), 
                            command.Prefix, "Not Set")).AppendLine()

                await ReplyAsync(Format.Code(output.ToString()))
            End Function
        
        End Class
    
    End Class
    
End NameSpace