Imports System.Text
Imports System.Timers
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Containers
Imports RailgunVB.Core.Logging
Imports TreeDiagram
Imports TreeDiagram.Models.TreeTimer

Namespace Core.Managers
    
    Public Class TimerManager
    
        Private ReadOnly _masterTimer As New Timer(TimeSpan.FromMinutes(10).TotalMilliseconds)
        Private ReadOnly _remindMeContainers As New List(Of RemindMeContainer)
        Private _initialized As Boolean = False
        
        Private ReadOnly _log As Log
        Private ReadOnly _services As IServiceProvider
        
        Public ReadOnly Property RemindMeContainerCount As Integer
            Get
                Return _remindMeContainers.Count
            End Get
        End Property

        Public Sub New(services As IServiceProvider)
            _log = services.GetService(Of Log)
            _services = services
            
            AddHandler _masterTimer.Elapsed, Async Sub() Await MasterTimerElapsed()
            _masterTimer.AutoReset = True
        End Sub
        
        Public Async Function Initialize() As Task
            Await _log.LogToBotLogAsync($"{If(_initialized, "Re-", "")}Initializing...", BotLogType.TimerManager)
            
            If _initialized
                _masterTimer.Stop()
                _remindMeContainers.ForEach(Sub(container) container.StopTimer())                
                _remindMeContainers.Clear()
            End If
            
            Dim newTimers = 0
            Dim crashedTimers = 0
            Dim completedTimers = 0
            
            Using db As TreeDiagramContext = _services.GetService(Of TreeDiagramContext)
                For Each data As TimerRemindMe In db.TimerRemindMes
                    If data.TimerExpire < DateTime.UtcNow
                        Dim container As New RemindMeContainer(_services, data)
                        
                        Await container.ExecuteOverride()
                        
                        If container.IsCompleted
                            completedTimers += 1
                        ElseIf container.HasCrashed
                            crashedTimers += 1
                        End If
                        Continue For
                    ElseIf Await CreateAndStartRemindMeContainer(data)
                        newTimers += 1
                    End If
                Next
            End Using
            
            _masterTimer.Start()
            
            Dim output As New StringBuilder
            
            output.AppendFormat("{0}Initialization Completed!", If(_initialized, "Re-", "")).AppendLine() _
                .AppendLine() _
                .AppendFormat("Timers Executed & Cleaned Up : {0}", completedTimers).AppendLine() _
                .AppendFormat("Timers Crashed & Cleaned Up  : {0}", crashedTimers).AppendLine() _
                .AppendFormat("Timers Started               : {0}", newTimers)
            
            Await _log.LogToBotLogAsync(output.ToString(), BotLogType.TimerManager)
            _initialized = True
        End Function
        
        Private Async Function MasterTimerElapsed() As Task
            Dim newTimers = 0
            Dim crashedTimers = 0
            Dim completedTimers = 0
            Dim index = 0
            
            While index < RemindMeContainerCount
                Dim container As RemindMeContainer = _remindMeContainers(index)
                
                If container.IsCompleted OrElse container.HasCrashed
                    If container.IsCompleted 
                        completedTimers += 1
                    Else 
                        crashedTimers += 1
                    End If
                    _remindMeContainers.RemoveAt(index)
                    index -= 1
                End If
                
                index += 1
            End While
            
            Using db As TreeDiagramContext = _services.GetService(Of TreeDiagramContext)
                For Each data As TimerRemindMe In db.TimerRemindMes
                    If Await CreateAndStartRemindMeContainer(data) Then newTimers += 1
                Next
            End Using
            
            If newTimers = 0 AndAlso completedTimers = 0 AndAlso crashedTimers = 0 Then Return
            
            Dim output As New StringBuilder
            
            output.AppendLine("Timer Housekeeping Completed!") _
                .AppendLine() _
                .AppendFormat("Started         : {0}", newTimers).AppendLine() _
                .AppendFormat("Already Running : {0}", RemindMeContainerCount - newTimers).AppendLine() _
                .AppendFormat("Crashed/Errored : {0}", crashedTimers).AppendLine() _
                .AppendFormat("Final Cleanup   : {0}", completedTimers + crashedTimers)
            
            Await _log.LogToBotLogAsync(output.ToString(), BotLogType.TimerManager)
        End Function
        
        Public Async Function CreateAndStartRemindMeContainer(data As TimerRemindMe, 
                                                              Optional isNew As Boolean = False) As Task(Of Boolean)
            Dim remainingTime As TimeSpan = data.TimerExpire - DateTime.UtcNow
            
            If remainingTime.TotalMinutes < 30 AndAlso _remindMeContainers.FirstOrDefault(
                Function(find) find.Data.Id = data.Id) Is Nothing
                Dim container As New RemindMeContainer(_services, data)
                
                container.StartTimer(remainingTime.TotalMilliseconds)
                
                _remindMeContainers.Add(container)
                
                If isNew Then Await _log.LogToBotLogAsync(
                    $"Remind Me || Timer #{data.Id} Created & Started!", BotLogType.TimerManager)
                
                Return True
            ElseIf isNew
                Await _log.LogToBotLogAsync($"Remind Me || Timer #{data.Id} Created!", BotLogType.TimerManager)
                Return False
            Else
                Return False
            End If
        End Function
        
    End Class
    
End NameSpace