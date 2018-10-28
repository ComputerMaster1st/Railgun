Imports System.Text
Imports Discord
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Enums
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Core.Preconditions
    
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method)>
    Public NotInheritable Class RoleLock
        Inherits PreconditionAttribute
        
        Private ReadOnly _moduleType As ModuleType
        
        Public Sub New(moduleType As ModuleType)
            _moduleType = moduleType
        End Sub

        Public Overrides Async Function CheckPermissionsAsync(context As ICommandContext, command As CommandInfo, 
                                                        services As IServiceProvider) As Task(Of PreconditionResult)
            Dim user As IGuildUser = context.User
            Dim output As New StringBuilder
            
            Using db As TreeDiagramContext = services.GetService(Of TreeDiagramContext)
                Select _moduleType
                    Case ModuleType.Music
                        Dim data As ServerMusic = Await db.ServerMusics.GetAsync(context.Guild.Id)
                        
                        If data Is Nothing OrElse data.AllowedRoles.Count < 1
                            Return Await Task.FromResult(PreconditionResult.FromSuccess())
                        End If
                        
                        Dim tempOutput As New StringBuilder
                        
                        For Each allowedRole As AllowedRole In data.AllowedRoles
                            Dim role As IRole = context.Guild.GetRole(allowedRole.RoleId)
                            
                            tempOutput.AppendLine($"| {role.Name} |")
                            
                            If user.RoleIds.Contains(allowedRole.RoleId)
                                Return Await Task.FromResult(PreconditionResult.FromSuccess())
                            End If
                        Next
                        
                        output.AppendLine("This command is locked to specific role(s). You must have the following role(s)...") _ 
                            .AppendLine() _
                            .AppendLine(tempOutput.ToString())
                        
                        Exit Select
                End Select
            End Using
            
            Return Await Task.FromResult(PreconditionResult.FromError(output.ToString()))
        End Function
        
    End Class
    
End NameSpace