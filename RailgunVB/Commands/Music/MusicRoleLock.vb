Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core
Imports RailgunVB.Core.Preconditions
Imports TreeDiagram
Imports TreeDiagram.Models.Server

Namespace Commands.Music
    
    Partial Public Class Music
    
        <Group("rolelock"), UserPerms(GuildPermission.ManageGuild)>
        Public Class MusicRoleLock
            Inherits SystemBase
            
            <Command("add")>
            Public Async Function AddRoleAsync(<Remainder> roleName As String) As Task
                For Each role As IRole In Context.Guild.Roles
                    If role.Name.Contains(roleName)
                        Await AddRoleAsync(role)
                        Return
                    End If
                Next
                
                Await ReplyAsync("Unable to find a role with the name you specified.")
            End Function
    
            <Command("add")>
            Public Async Function AddRoleAsync(role As IRole) As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                
                data.AllowedRoles.Add(New AllowedRole(role.Id))
                
                If data.AllowedRoles.Count < 2
                    Await ReplyAsync($"All music commands are now role-locked to {role.Name}.")
                    Return
                End If
                
                Await ReplyAsync($"Users with the role {Format.Bold(role.Name)}, may also use the music commands.")
            End Function
            
            <Command("remove")>
            Public Async Function RemoveRoleAsync(<Remainder> roleName As String) As Task
                For Each role As IRole In Context.Guild.Roles
                    If role.Name.Contains(roleName)
                        Await RemoveRoleAsync(role)
                        Return
                    End If
                Next
                
                Await ReplyAsync("Unable to find a role with the name you specified.")
            End Function
            
            <Command("remove")>
            Public Async Function RemoveRoleAsync(role As IRole) As Task
                Dim data As ServerMusic = Await Context.Database.ServerMusics.GetOrCreateAsync(Context.Guild.Id)
                Dim count As Integer = data.AllowedRoles.RemoveAll(function(allowedRole) allowedRole.RoleId = role.Id)
                
                If count < 1
                    Await ReplyAsync("The role specified was never role-locked.")
                    Return
                End If
                
                Dim output As New StringBuilder
                
                output.AppendFormat("The role {0}, has now been removed from role-locking.", role.Name).AppendLine()
                
                If data.AllowedRoles.Count < 1 Then output.AppendLine("All music commands are no longer role-locked.")
                
                Await ReplyAsync(output.ToString())
            End Function
            
        End Class
        
    End Class
    
End NameSpace