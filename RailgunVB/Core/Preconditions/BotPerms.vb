Imports Discord
Imports Discord.Commands
Imports Microsoft.Extensions.DependencyInjection
Imports RailgunVB.Core.Configuration

Namespace Core.Preconditions
    
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple := True)>
    Public NotInheritable Class BotPerms
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
            Dim self As IGuildUser = Await context.Guild.GetCurrentUserAsync()
            
            If _guildPermission.HasValue
                If Not (self.GuildPermissions.Has(_guildPermission) OrElse self.GuildPermissions.Administrator OrElse 
                   config.DiscordConfig.OtherAdmins.Contains(self.Id) OrElse 
                   self.Id = config.DiscordConfig.MasterAdminId)
                    Return Await Task.FromResult(PreconditionResult.FromError(
                        $"I do not have permission to perform this command! {Format.Bold(
                            $"SERVER-PERM-MISSING : {_guildPermission.ToString()}")}"))
                End If
            ElseIf _channelPermission.HasValue
                Dim channelPerms As ChannelPermissions = self.GetPermissions(context.Channel)
                
                If Not (channelPerms.Has(_channelPermission.Value) OrElse self.GuildPermissions.Administrator OrElse 
                        config.DiscordConfig.OtherAdmins.Contains(self.Id) OrElse 
                        self.Id = config.DiscordConfig.MasterAdminId) 
                    Return Await Task.FromResult(PreconditionResult.FromError(
                        $"I do not have permission to perform this command! {Format.Bold(
                            $"CHANNEL-PERM-MISSING : {_channelPermission.ToString()}")}"))
                End If
            End If
            
            Return Await Task.FromResult(PreconditionResult.FromSuccess())
        End Function
        
    End Class
    
End NameSpace