Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Configuration

Namespace Commands.Root
    Partial Public Class Root
    
        <Group("admin")>
        Public Class RootAdmin
            Inherits SystemBase
        
            Private ReadOnly _config As MasterConfig

            Public Sub New(config As MasterConfig)
                _config = config
            End Sub
            
            <Command("add")>
            Public Async Function AddAsync(user As IUser) As Task
                If _config.DiscordConfig.OtherAdmins.Contains(user.Id)
                    await ReplyAsync($"{Format.Bold(
                        $"{user.Username}#{user.DiscriminatorValue}")} is already a Railgun Admin.")
                Else 
                    await _config.AssignAdminAsync(user.Id)
                    await ReplyAsync($"Assigned {Format.Bold(
                        $"{user.Username}#{user.DiscriminatorValue}")} as a Railgun Admin.")
                End If
            End Function
            
            <Command("remove")>
            Public Async Function RemoveAsync(user As IUser) As Task
                If _config.DiscordConfig.OtherAdmins.Contains(user.Id)
                    await _config.RemoveAdminAsync(user.Id)
                    await ReplyAsync($"{Format.Bold(
                        $"{user.Username}#{user.DiscriminatorValue}")} is no longer a Railgun Admin.")
                Else
                    await ReplyAsync($"{Format.Bold(
                        $"{user.Username}#{user.DiscriminatorValue}")} was never a Railgun Admin.")
                End If
            End Function
            
        End Class
    
    End Class
    
End NameSpace