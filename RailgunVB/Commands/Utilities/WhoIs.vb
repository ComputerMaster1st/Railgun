Imports System.Text
Imports Discord
Imports Discord.Commands
Imports RailgunVB.Core

Namespace Commands.Utilities
    
    <Group("whois")>
    Public Class WhoIs
        Inherits SystemBase
        
        <Command>
        Public Async Function WhoisAsync(userId As ULong) As Task
            Dim user As IUser = Await Context.Client.GetUserAsync(userId)
            
            If user Is Nothing
                Await ReplyAsync("Can not find user.")
                Return
            End If
            
            Await WhoisAsync(user)
        End Function
        
        <Command>
        Public Async Function WhoisAsync(Optional pUser As IUser = Nothing) As Task
            Dim user As IUser = If(pUser Is Nothing, Context.User, pUser)
            Dim username As String = $"{user.Username}#{user.DiscriminatorValue}"
            Dim isHuman As String = If(user.IsBot Or user.IsWebhook, "Bot", "User")
            Dim guilds As IReadOnlyCollection(Of IGuild) = Await Context.Client.GetGuildsAsync()
            Dim locatedOn = 0
            Dim builder As New EmbedBuilder With {
                    .Title = $"Who is {Format.Bold(username)}?",
                    .Color = Color.Blue,
                    .ThumbnailUrl = user.GetAvatarUrl(),
                    .Footer = new EmbedFooterBuilder() With {
                        .Text = $"User ID : {user.Id}"
                    }
                }
            
            For Each guild As IGuild In guilds
                If (Await guild.GetUserAsync(user.Id)) IsNot Nothing Then locatedOn += 1
            Next
            
            builder.AddField("User Type:", isHuman, True) _
                .AddField("Registered:", user.CreatedAt, True) _
                .AddField("Located On:", locatedOn, True)
            
            Dim gUser As IGuildUser = Await Context.Guild.GetUserAsync(user.Id)
            
            If gUser IsNot Nothing
                Dim roles As New StringBuilder
                
                For Each roleId As ULong In gUser.RoleIds
                    roles.AppendFormat("| {0} |", Context.Guild.GetRole(roleId).Mention)
                Next
                
                builder.AddField("Server Nickname:", If(gUser.Nickname, "N/A"), true) _
                    .AddField("Joined Server At:", If(gUser.JoinedAt, "UNKNOWN"), true) _
                    .AddField("Current Server Roles:", If(roles.ToString(), "N/A")) _
                    .AddField("ESPer Level:", GetEsperLevel(gUser))
            End If
            
            Await ReplyAsync("", embed := builder.Build())
        End Function
        
        Private Function GetEsperLevel(gUser As IGuildUser) As Integer
            If Context.Guild.OwnerId = gUser.Id Then Return 6
            
            Dim score = 0
            
            For Each perm As GuildPermission In gUser.GuildPermissions.ToList()
                Select perm
                    Case GuildPermission.Administrator
                        Return 5
                    Case GuildPermission.ManageChannels, GuildPermission.ManageEmojis, GuildPermission.ManageGuild, 
                        GuildPermission.ManageRoles, GuildPermission.ManageWebhooks, GuildPermission.ViewAuditLog,
                        GuildPermission.ManageNicknames, GuildPermission.ManageMessages
                        score += 10
                        Exit Select
                    Case GuildPermission.BanMembers, GuildPermission.DeafenMembers, GuildPermission.KickMembers, 
                        GuildPermission.MoveMembers, GuildPermission.MuteMembers, GuildPermission.MentionEveryone, 
                        GuildPermission.SendTTSMessages, GuildPermission.PrioritySpeaker
                        score += 5
                        Exit Select
                    Case GuildPermission.UseVAD, GuildPermission.UseExternalEmojis, GuildPermission.AttachFiles, 
                        GuildPermission.CreateInstantInvite, GuildPermission.AddReactions, GuildPermission.ChangeNickname,
                        GuildPermission.EmbedLinks, GuildPermission.ReadMessageHistory
                        score += 2
                        Exit Select
                    Case GuildPermission.SendMessages, GuildPermission.ViewChannel, GuildPermission.Connect, 
                        GuildPermission.Speak
                        score += 1
                        Exit Select
                End Select
            Next
            
            Dim percent As Double = Math.Round((score / 140) * 100)
            
            Select percent
                Case Is < 10
                    Return 0
                Case Is < 20
                    Return 1
                Case Is < 40
                    Return 2
                Case Is < 60
                    Return 3
                Case Else
                    Return 4
            End Select
        End Function
    
    End Class
    
End NameSpace