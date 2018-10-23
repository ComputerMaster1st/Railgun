Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    Partial Public Class Music
        
        <Group("np")>
        Public Class MusicNp
            Inherits SystemBase
        
            Private ReadOnly _playerManager As PlayerManager
            
            Public Sub New(playerManager As PlayerManager)
                _playerManager = playerManager
            End Sub
            
            <Command>
            Public Async Function NowPlayingAsync() As Task
                Dim player As Player = _playerManager.GetPlayer(Context.Guild.Id).Player
            
                If player Is Nothing
                    await ReplyAsync("I'm not playing anything at this time.")
                    Return
                End If
            
                Dim meta As SongMetadata = player.GetFirstSongRequest().Metadata
                Dim output As New StringBuilder
            
                output.AppendFormat("Currently playing {0} at the moment.", Format.Bold(meta.Name)).AppendLine() _
                    .AppendFormat("Url: {0} || Length: {1}", Format.Bold($"<{meta.Url}>"), Format.Bold(meta.Length.ToString()))
            
                await ReplyAsync(output.ToString())
            End Function
            
            <Command("channel"), UserPerms(GuildPermission.ManageGuild)>
            Public Async Function SetNpChannelAsync(Optional tcParam As ITextChannel = Nothing) As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                Dim tc As ITextChannel = If(tcParam, Context.Channel)
                    
                If data.NowPlayingChannel <> 0 AndAlso tc.Id = data.NowPlayingChannel
                    Await SetNpChannelAsync(data, tc)
                    Return
                End If
                        
                Await SetNpChannelAsync(data, tc, True)
            End Function
            
            Private Async Function SetNpChannelAsync(data As ServerMusic, tc As ITextChannel, 
                                                              Optional locked As Boolean = False) As Task
                data.NowPlayingChannel = If(locked, tc.Id, 0)
                Await ReplyAsync($"{Format.Bold("Now Playing")} messages are {Format.Bold(
                    If(locked, "Now", "No Longer"))} locked to #{tc.Name}.")
            End Function
            
        End Class
        
    End Class

End NameSpace