Imports System.Text
Imports System.Timers
Imports Discord
Imports RailgunVB.Core.Logging
Imports TreeDiagram
Imports TreeDiagram.Models.TreeTimer

Namespace Core.Containers
    
    Public Class RemindMeContainer
    
        Private ReadOnly _log As Log
        Private ReadOnly _client As IDiscordClient
        Private ReadOnly _dbContext As TreeDiagramContext
        
        Public ReadOnly Property Data As TimerRemindMe
        Public ReadOnly Property IsCompleted As Boolean = False
        Public ReadOnly Property HasCrashed As Boolean = False
        
        Public Sub New(log As Log, client As IDiscordClient, dbContext As TreeDiagramContext, data As TimerRemindMe)
            _log = log
            _client = client
            _dbContext = dbContext
            Me.Data = data
        End Sub
        
        Public Sub StartTimer(ms As Double)
            Data.Timer = New Timer(ms)
            Data.Timer.AutoReset = False
            AddHandler Data.Timer.Elapsed, RemindMeElapsed
            Data.Timer.Start()
        End Sub

        Public Sub StopTimer()
            Data.Timer.Stop()
            Data.Timer.Dispose()
        End Sub
        
        Public Async Function ExecuteOverride() As Task
            Await RemindMeElapsed()
        End Function
        
        Private Async Function RemindMeElapsed() As Task
            Try
                Dim guild As IGuild = Await _client.GetGuildAsync(Data.GuildId)
                
                If guild Is Nothing Then Throw New NullReferenceException("RemindMe || Guild doesn't exist!")
                
                Dim tc As ITextChannel = Await guild.GetTextChannelAsync(Data.TextChannelId)
                Dim user As IGuildUser = Await guild.GetUserAsync(Data.UserId)
                
                If tc Is Nothing
                    Throw New NullReferenceException("RemindMe || Guild->TextChannel doesn't exist!")
                ElseIf user Is Nothing
                    Throw New NullReferenceException("RemindMe || Guild->User doesn't exist!")
                End If
                
                Dim output As New StringBuilder
                
                output.AppendFormat("{0}, you asked me to remind you about the following...", 
                                    user.Mention).AppendLine() _
                    .AppendLine() _
                    .AppendLine(Data.Message)
                
                Await tc.SendMessageAsync(output.ToString())
                
                _IsCompleted = True
                
                Await Dispose()
                Await _log.LogToBotLogAsync(
                    $"Remind Me || Timer ID {Data.Id.ToString()} has completed! Awaiting final cleanup.", 
                    BotLogType.TimerManager)
            Catch ex As NullReferenceException
                _HasCrashed = True
                
                Dispose.GetAwaiter()
                _log.LogToConsoleAsync(new LogMessage(
                    LogSeverity.Warning, "Timers", 
                    $"Timer ID |{Data.Id.ToString()}| RemindMe Container Exception!", ex)).GetAwaiter()
            Catch ex As Exception
                If Data.Timer IsNot Nothing AndAlso Not (IsCompleted)
                    StopTimer()
                    StartTimer(TimeSpan.FromSeconds(60).TotalMilliseconds)
                    Return
                ElseIf Data.Timer Is Nothing AndAlso Not (IsCompleted)
                    _HasCrashed = True
                    Dispose.GetAwaiter()
                End If
                
                _log.LogToConsoleAsync(new LogMessage(
                    LogSeverity.Warning, "Timers", 
                    $"Timer ID |{Data.Id.ToString()}| RemindMe Container Exception!", ex)).GetAwaiter()
            End Try
        End Function
        
        Private Async Function Dispose() As Task
            If Data.Timer IsNot Nothing
                Data.Timer.Dispose()
                Data.Timer = Nothing
            End If
            
            Await _dbContext.TimerRemindMes.DeleteAsync(Data.GuildId)
            Await _dbContext.SaveChangesAsync()
        End Sub
        
    End Class

End NameSpace