Imports Discord
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Configuration

Namespace Core.Preconditions
    
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method)>
    Public NotInheritable Class BotAdmin
        Inherits PreconditionAttribute

        Public Overrides Async Function CheckPermissionsAsync(context As ICommandContext, command As CommandInfo, services As IServiceProvider) As Task(Of PreconditionResult)
            Dim config As MasterConfig = services.GetService(Of MasterConfig)
            Dim user As IGuildUser = Await context.Guild.GetUserAsync(context.User.Id)
                
            If user.Id = config.DiscordConfig.MasterAdminId OrElse 
               config.DiscordConfig.OtherAdmins.Contains(user.Id) 
                Return Await Task.FromResult(PreconditionResult.FromSuccess())
            Else 
                Return Await Task.FromResult(PreconditionResult.FromError(
                    "You must be a Railgun Administrator to use this command."))
            End If
        End Function
                                   
    End Class
    
End NameSpace