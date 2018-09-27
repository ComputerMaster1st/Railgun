Imports System.Text
Imports Discord
Imports Discord.Commands

Namespace Commands
    
    <Group("whois")>
    Public Class WhoIs
        Inherits ModuleBase
        
        <Command>
        Public Async Function WhoisAsync(userId As ULong) As Task
            Dim user As IUser = Await Context.Client.GetUserAsync(userId)
            
            If user Is Nothing
                Await ReplyAsync("Can not find user.")
                Return
            End If
            
            Await WhoisAsync(user)
        End Function
        
        Public Async Function WhoisAsync(Optional pUser As IUser = Nothing) As Task
            Dim user As IUser = If(pUser Is Nothing, Context.User, pUser)
            Dim username As String = $"{user.Username}#{user.DiscriminatorValue}"
            Dim isHuman As String = If(user.IsBot Or user.IsWebhook, "Bot", "User")
            Dim builder As New EmbedBuilder With {
                    .Title = $"Who is {Format.Bold(username)}?",
                    .Color = Color.Blue,
                    .ThumbnailUrl = user.GetAvatarUrl(),
                    .Footer = new EmbedFooterBuilder() With {
                        .Text = $"User ID : {user.Id}"
                    }
                }
            
            builder.AddField("User Type:", isHuman, true) _
                .AddField("Registered:", user.CreatedAt, true)
            
            Dim gUser As IGuildUser = Await Context.Guild.GetUserAsync(user.Id)
            
            If gUser IsNot Nothing
                Dim roles As New StringBuilder
                Dim esperLevel As Integer = GetEsperLevel(gUser)
                
                For Each roleId As ULong In gUser.RoleIds
                    roles.AppendFormat("| {0} |", Context.Guild.GetRole(roleId).Mention)
                Next
                
                builder.AddField("Server Nickname:", If(gUser.Nickname, "N/A"), true) _
                    .AddField("Joined Server At:", gUser.JoinedAt, true) _
                    .AddField("Current Server Roles:", If(roles.ToString(), "N/A")) _
                    .AddField("ESPer Level:", esperLevel)
            End If
            
            Await ReplyAsync("", embed := builder.Build())
        End Function
        
        Private Function GetEsperLevel(gUser As IGuildUser) As Integer
            If Context.Guild.OwnerId = gUser.Id Then Return 6
            
            Dim score = 0
            
            For Each perm As GuildPermission In gUser.GuildPermissions.ToList()
                Select perm
                    Case GuildPermission.Administrator
                        score += 214
                        Exit Select
                    Case GuildPermission.ManageChannels
                    Case GuildPermission.ManageEmojis
                    Case GuildPermission.ManageGuild
                    Case GuildPermission.ManageRoles
                    Case GuildPermission.ManageWebhooks
                    Case GuildPermission.ViewAuditLog
                        score += 10
                        Exit Select
                    Case GuildPermission.BanMembers
                    Case GuildPermission.DeafenMembers
                    Case GuildPermission.KickMembers
                    Case GuildPermission.MoveMembers
                    Case GuildPermission.MuteMembers
                    Case GuildPermission.ManageMessages
                    Case GuildPermission.ManageNicknames
                    Case GuildPermission.PrioritySpeaker
                    Case GuildPermission.MentionEveryone
                        score += 5
                        Exit Select
                    Case GuildPermission.AddReactions
                    Case GuildPermission.AttachFiles
                    Case GuildPermission.ChangeNickname
                    Case GuildPermission.CreateInstantInvite
                    Case GuildPermission.EmbedLinks
                    Case GuildPermission.ReadMessageHistory
                    Case GuildPermission.SendMessages
                    Case GuildPermission.SendTTSMessages
                    Case GuildPermission.UseExternalEmojis
                    Case GuildPermission.ViewChannel
                    Case GuildPermission.Connect
                    Case GuildPermission.Speak
                    Case GuildPermission.UseVAD
                        score += 1
                        Exit Select
                End Select
            Next
            
            Dim percent As Integer = Math.Round((score / 214) * 100)
            
            Select percent
                Case >= 100
                    Return 5
                Case >= 80
                    Return 4
                Case >= 60
                    Return 3
                Case >= 40
                    Return 2
                Case >= 20
                    Return 1
                Case Else
                    Return 0
            End Select
        End Function
    
    End Class
    
End NameSpace