Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    Partial Public Class Music
        
        <Group("vote"), UserPerms(GuildPermission.ManageGuild)>
        Public Class MusicVote
            Inherits ModuleBase
            
            Private ReadOnly _dbContext As TreeDiagramContext

            Public Sub New(dbContext As TreeDiagramContext)
                _dbContext = dbContext
            End Sub
            
            Protected Overrides Async Sub AfterExecute(command As CommandInfo)
                Await _dbContext.SaveChangesAsync()
                MyBase.AfterExecute(command)
            End Sub
            
            <Command>
            Public Async Function EnableAsync() As Task
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                
                data.VoteSkipEnabled = Not (data.VoteSkipEnabled)
                
                await ReplyAsync($"Music Vote-Skip is now {If(data.VoteSkipEnabled, Format.Bold(
                    $"Enabled @ {data.VoteSkipLimit}%"), Format.Bold("Disabled"))}.")
            End Function
            
            <Command("percent")>
            Public Async Function SetPercentAsync(percent As Integer) As Task
                If percent < 10 OrElse percent > 100
                    await ReplyAsync("Percentage must be set between 10-100.")
                    Return
                End If
                
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                
                data.VoteSkipLimit = percent
                
                If Not (data.VoteSkipEnabled) Then data.VoteSkipEnabled = True
                
                await ReplyAsync($"Music Vote-Skip is now {If(Not (data.VoteSkipEnabled), 
                    Format.Bold("enabled &"), "")} set to skip songs when {data.VoteSkipLimit}% of users have voted.")
            End Function
            
        End Class
    
    End Class
    
End NameSpace