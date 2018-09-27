Imports Discord
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Configuration

Namespace Core.Preconditions
    
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple := True)>
    Public NotInheritable Class UserPerms
        Inherits PreconditionAttribute
    
        Private ReadOnly _guildPermission As GuildPermission? = Nothing
        Private ReadOnly _channelPermission As ChannelPermission? = Nothing
        
        Public Sub New(guildPermission As GuildPermission)
             _guildPermission = guildPermission
        End Sub
        
        Public Sub New(channelPermission As ChannelPermission)
            _channelPermission = channelPermission
        End Sub

        Public Overrides Async Function CheckPermissionsAsync(context As ICommandContext, command As CommandInfo, services As IServiceProvider) As Task(Of PreconditionResult)
            Dim config As MasterConfig = services.GetService(Of MasterConfig)
            Dim user As IGuildUser = Await context.Guild.GetUserAsync(context.User.Id)
            
            If _guildPermission.HasValue
                If Not (user.GuildPermissions.Has(_guildPermission) OrElse user.GuildPermissions.Administrator OrElse 
                   config.DiscordConfig.OtherAdmins.Contains(user.Id) OrElse 
                   user.Id = config.DiscordConfig.MasterAdminId)
                    Return Await Task.FromResult(PreconditionResult.FromError(
                        $"You do not have permission to use this command! {Format.Bold(
                            $"SERVER-PERM-MISSING : {_guildPermission.ToString()}")}"))
                End If
            ElseIf _channelPermission.HasValue
                Dim channelPerms As ChannelPermissions = user.GetPermissions(context.Channel)
                
                If Not (channelPerms.Has(_channelPermission.Value) OrElse user.GuildPermissions.Administrator OrElse 
                        config.DiscordConfig.OtherAdmins.Contains(user.Id) OrElse 
                        user.Id = config.DiscordConfig.MasterAdminId) 
                    Return Await Task.FromResult(PreconditionResult.FromError(
                        $"You do not have permission to use this command! {Format.Bold(
                            $"CHANNEL-PERM-MISSING : {_channelPermission.ToString()}")}"))
                End If
            End If
            
            Return Await Task.FromResult(PreconditionResult.FromSuccess())
        End Function
        
    End Class
    
End NameSpace