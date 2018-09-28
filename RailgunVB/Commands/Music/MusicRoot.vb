Imports System.IO
Imports System.Text
Imports AudioChord
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core.Managers
Imports RailgunVB.Core.Music
Imports RailgunVB.Core.Preconditions

Namespace Commands.Music

    Partial Public Class Music
    
        <Group("root"), BotAdmin>
        Public Class MusicRoot
            Inherits ModuleBase
            
            Private ReadOnly _playerManager As PlayerManager
            Private ReadOnly _musicService As MusicService

            Public Sub New(playerManager As PlayerManager, musicService As MusicService)
                _playerManager = playerManager
                _musicService = musicService
            End Sub
            
            <Command("active"), BotPerms(ChannelPermission.AttachFiles)>
            Public Async Function ActiveAsync() As Task
                If _playerManager.ActivePlayers.Count < 1
                    await ReplyAsync("There are no active music streams at this time.")
                    Return
                End If
                
                Dim output As New StringBuilder
                
                output.AppendFormat("Active Music Streams ({0} Total):", 
                                    _playerManager.ActivePlayers.Count).AppendLine().AppendLine()
                
                For Each info In _playerManager.ActivePlayers
                    Dim player As Player = info.Value.Item2
                    
                    output.AppendFormat("Id : {0} || Spawned At : {1} || Status : {2}", info.Key, player.CreatedAt, 
                                        player.Status).AppendLine() _ 
                        .AppendFormat("\\--> Latency : {0}ms || Playing : {1} || Since : {2}", player.Latency, 
                                      player.GetFirstSongRequest(), player.SongStartedAt).AppendLine().AppendLine()
                Next
                
                If output.Length < 1950
                    await ReplyAsync(Format.Code(output.ToString()))
                    Return
                End If
                
                Const filename As String = "ActivePlayers.txt"
                
                await File.WriteAllTextAsync(filename, output.ToString())
                await Context.Channel.SendFileAsync(filename)
                File.Delete(filename)
            End Function
            
            <Command("kill")>
            Public Async Function KillAsync(id As ULong) As Task
                _playerManager.DisconnectPlayer(id)
                await ReplyAsync($"Sent 'Kill Code' to Player ID {id}.")
            End Function
            
            <Command("queue-restart")>
            Public Async Function RestartQueueAsync() As Task
                Dim status As QueueProcessorStatus = _musicService.RestartQueueProcessorAsync()
                Dim output As String
                
                Select status
                    Case QueueProcessorStatus.Idle
                        output = "Global Queue Processor is currently idling."
                        Exit Select
                    Case QueueProcessorStatus.Running
                        output = "Global Queue Processor is currently running."
                        Exit Select
                    Case QueueProcessorStatus.Restarted
                        output = "Global Queue Processor has now been restarted."
                        Exit Select
                End Select
                
                await ReplyAsync(output)
            End Function
            
        End Class
    
    End Class
    
End NameSpace