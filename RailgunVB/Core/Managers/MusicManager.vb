Imports System.Text
Imports AudioChord
Imports AudioChord.Events
Imports Discord
Imports RailgunVB.Core.Logging
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Core.Managers
    
    Public Class MusicManager
        
        Private ReadOnly _log As Log
        
        Private ReadOnly _client As IDiscordClient
        
        Private ReadOnly _dbContext As TreeDiagramContext
        Private WithEvents _musicService As MusicService
        
        Public Sub New(log As Log, client As IDiscordClient, dbContext As TreeDiagramContext, 
                       musicService As MusicService)
            _log = log
            _client = client
            _dbContext = dbContext
            _musicService = musicService
        End Sub
        
        Private Async Function SongProcessingCompletedAsync(sender As Object, 
                                                            args As SongProcessingCompletedEventArgs) As Task _ 
                                                            Handles _musicService.SongProcessingCompleted
            Await _log.LogToBotLogAsync("Global Song Processor Completed!", BotLogType.AudioChord)
        End Function
        
        Private Async Function SongsExistAsync(sender As Object, 
                                               args As SongsExistedEventArgs) As Task Handles _musicService.SongsExisted
            Try
                Dim guild As IGuild = Await _client.GetGuildAsync(args.GuildId)
                Dim tc As ITextChannel = Await guild.GetTextChannelAsync(args.TextChannelId)
                Dim output As New StringBuilder
                
                output.AppendFormat("{0} |", If(args.QueuedSongsCount < 1, 
                                                "Processing Completed!", "Processing Songs...")) _
                    .AppendFormat("| Accepted : {0} |", Format.Bold(args.InstalledExistingSongsCount.ToString())) _
                    .AppendFormat("| Already Installed : {0} |", 
                                  Format.Bold(args.AlreadyInstalledSongsCount.ToString())) _
                    .AppendFormat("{0}", If(args.QueuedSongsCount > 0, 
                                            $"| In Queue {Format.Bold(args.QueuedSongsCount.ToString())} |", "")) _
                    .AppendFormat("| Failed : {0}", Format.Bold(args.FailedParsingSongsCount.ToString())).AppendLine() _
                    .AppendLine() _              
                    .AppendFormat("{0}", (args.QueuedSongsCount > 0, Format.Bold("NOTE : The time to process the queue shall vary depending on how many songs are already queued for processing. Please wait patiently for all your songs to complete processing."), ""))
                Await tc.SendMessageAsync(output.ToString())
            Catch
                _log.LogToBotLogAsync($"<({args.GuildId})> Guild/TC Missing?", BotLogType.AudioChord).GetAwaiter()
                _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "AudioChord", 
                                                      $"<({args.GuildId})> Guild/TC Missing?")).GetAwaiter()
            End Try
        End Function
        
        Private Async Function ProcessSongAsync(sender As Object, args As ProcessedSongEventArgs) As Task _ 
                                                Handles _musicService.ProcessedSong
            Try
                Dim guild As IGuild = Await _client.GetGuildAsync(args.GuildId)
                Dim tc As ITextChannel = Await guild.GetTextChannelAsync(args.TextChannelId)
                Dim data As ServerMusic = Await _dbContext.ServerMusics.GetOrCreateAsync(guild.Id)
                
                If Not (data.SilentSongProcessing)
                    Dim tcOutput As New StringBuilder
                    
                    tcOutput.AppendFormat("{0} |", If(string.IsNullOrEmpty(args.SongName), 
                                                      "Process Failure!", "Processed Song!")) _
                        .AppendFormat("| ID : {0} |", Format.Bold(args.SongId)) _
                        .AppendFormat("| Remaining : {0} |", Format.Bold(args.SongsLeftToProcess.ToString())) _
                        .AppendFormat("| Queue Length : {0}", Format.Bold(args.SongInQueue.ToString())).AppendLine() _
                        .AppendFormat("{0}", If(Not (string.IsNullOrEmpty(args.SongName)), 
                                                $"Name : {Format.Bold(args.SongName)}", ""))
                    
                    Await tc.SendMessageAsync(tcOutput.ToString())
                End If
                
                Dim botOutput As New StringBuilder
                
                botOutput.AppendFormat("<{0} ({1})> {2}", guild.Name, guild.Id, 
                                       If(String.IsNullOrEmpty(args.SongName), "Process Failure!", 
                                          "Processed Song!")).AppendLine() _
                    .AppendFormat("| ID : {0} |", args.SongId) _
                    .AppendFormat("| Remaining : {0} |", args.SongsLeftToProcess) _
                    .AppendFormat("| Queue Length : {0}", args.SongInQueue)

                Await _log.LogToBotLogAsync(botOutput.ToString(), BotLogType.AudioChord)
                
                If args.SongsLeftToProcess < 1 Then Await tc.SendMessageAsync("Processing Completed!")
            Catch
                _log.LogToBotLogAsync($"<({args.GuildId})> Guild/TC Missing?", BotLogType.AudioChord).GetAwaiter()
                _log.LogToConsoleAsync(new LogMessage(LogSeverity.Warning, "AudioChord", 
                                                      $"<({args.GuildId})> Guild/TC Missing?")).GetAwaiter()
            End Try
        End Function
        
        Private Async Function ResyncAsync(sender As Object, 
                                           args As ResyncEventArgs) As Task Handles _musicService.ExecutedResync
            Dim output As New StringBuilder
            
            output.AppendLine("Database Resync Completed!") _
                .AppendFormat("---- Started At         : {0}", args.StartedAt).AppendLine() _
                .AppendFormat("---- Songs Expired      : {0}", args.DeletedExpiredSongs).AppendLine() _
                .AppendFormat("---- Songs Desynced     : {0}", args.DeletedDesyncedFiles).AppendLine() _
                .AppendFormat("---- Playlists Resynced : {0}", args.ResyncedPlaylists).AppendLine()

            Await _log.LogToBotLogAsync(output.ToString(), BotLogType.AudioChord)
        End Function
        
    End Class
    
End NameSpace