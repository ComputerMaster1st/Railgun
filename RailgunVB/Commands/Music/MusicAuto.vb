Imports Discord
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    Partial Public Class Music
        
        <Group("auto"), UserPerms(GuildPermission.ManageGuild)>
        Public Class MusicAuto
            Inherits SystemBase
            
            <Command("join")>
            Public Async Function JoinAsync() As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                Dim vc As IVoiceChannel = (CType(Context.User, IGuildUser)).VoiceChannel
                    
                If vc Is Nothing AndAlso data.AutoVoiceChannel = 0
                    Await ReplyAsync("Music Auto-Join is currently disabled. Please join a voice channel and run this command again to enable it.")
                    Return
                ElseIf vc Is Nothing AndAlso data.AutoVoiceChannel <> 0
                    data.AutoVoiceChannel = 0
                    data.AutoTextChannel = 0
                        
                    await ReplyAsync("Music Auto-Join has been disabled.")
                    Return
                End If
                    
                data.AutoVoiceChannel = vc.Id
                data.AutoTextChannel = Context.Channel.Id
                    
                await ReplyAsync($"{If(data.AutoVoiceChannel = 0, "Music Auto-Join is now enabled!", "") _ 
                    } Will automatically join {Format.Bold(vc.Name)} and use {Format.Bold(
                        "#" + Context.Channel.Name)} to post status messages.")
            End Function
            
            <Command("skip")>
            Public Async Function SkipAsync() As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                
                data.AutoSkip = Not (data.AutoSkip)
                
                await ReplyAsync($"Music Auto-Skip is now {Format.Bold(If(data.AutoSkip, "Enabled", "Disabled"))}.")
            End Function
            
            <Command("download")>
            Public Async Function DownloadAsync() As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                
                data.AutoDownload = Not (data.AutoDownload)
                
                Await ReplyAsync($"Music Auto-Download is now {Format.Bold(If(data.AutoDownload, "Enabled", 
                                                                              "Disabled"))}.")
            End Function
            
        End Class
        
    End Class
    
End NameSpace