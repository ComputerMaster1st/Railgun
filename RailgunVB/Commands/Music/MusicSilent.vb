Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    Partial Public Class Music
        
        <Group("silent"), UserPerms(GuildPermission.ManageGuild)>
        Public Class MusicSilent
            Inherits ModuleBase
            
            Private ReadOnly _dbContext As TreeDiagramContext

            Public Sub New(dbContext As TreeDiagramContext)
                _dbContext = dbContext
            End Sub
            
            Protected Overrides Async Sub AfterExecute(command As CommandInfo)
                Await _dbContext.SaveChangesAsync()
                MyBase.AfterExecute(command)
            End Sub
            
            <Command("running")>
            Public Async Function RunningAsync() As Task
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                
                data.SilentNowPlaying = Not (data.SilentNowPlaying)
                
                await ReplyAsync($"{Format.Bold(If(data.SilentNowPlaying, "Engaged", "Disengaged"))} Silent Running!")
            End Function
            
            <Command("install")>
            Public Async Function InstallAsync() As Task
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                
                data.SilentSongProcessing = Not (data.SilentSongProcessing)
                
                await ReplyAsync($"{Format.Bold(
                    If(data.SilentSongProcessing, "Engaged", "Disengaged"))} Silent Installation!")
            End Function
        End Class
        
    End Class
    
End NameSpace