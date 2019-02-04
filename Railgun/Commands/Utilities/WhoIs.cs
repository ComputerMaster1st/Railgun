using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;

namespace Railgun.Commands.Utilities
{
    [Alias("whois")]
    public class WhoIs : SystemBase
    {
        private int GetEsperLevel(IGuildUser user) {
            if (Context.Guild.OwnerId == user.Id) return 6;

            var score = 0;

            foreach (var perm in user.GuildPermissions.ToList()) {
                switch (perm) {
                    case GuildPermission.Administrator:
                        return 5;
                    case GuildPermission.ManageChannels:
                    case GuildPermission.ManageEmojis:
                    case GuildPermission.ManageGuild:
                    case GuildPermission.ManageRoles:
                    case GuildPermission.ManageWebhooks:
                    case GuildPermission.ViewAuditLog:
                    case GuildPermission.ManageNicknames:
                    case GuildPermission.ManageMessages:
                        score += 10;
                        break;
                    case GuildPermission.BanMembers:
                    case GuildPermission.DeafenMembers:
                    case GuildPermission.KickMembers:
                    case GuildPermission.MoveMembers:
                    case GuildPermission.MuteMembers:
                    case GuildPermission.MentionEveryone:
                    case GuildPermission.SendTTSMessages:
                    case GuildPermission.PrioritySpeaker:
                        score += 5;
                        break;
                    case GuildPermission.UseVAD:
                    case GuildPermission.UseExternalEmojis:
                    case GuildPermission.AttachFiles:
                    case GuildPermission.CreateInstantInvite:
                    case GuildPermission.AddReactions:
                    case GuildPermission.ChangeNickname:
                    case GuildPermission.EmbedLinks:
                    case GuildPermission.ReadMessageHistory:
                        score += 2;
                        break;
                    default:
                        score += 1;
                        break;
                }
            }

            var percent = Math.Round((score / 140) * 100.00);

            if (percent < 10) return 0;
            else if (percent < 20) return 1;
            else if (percent < 40) return 2;
            else if (percent < 60) return 3;
            else return 4;
        }

        [Command]
        public Task WhoIsAsync() => WhoIsAsync(Context.Author);

        [Command]
        public async Task WhoIsAsync(ulong userId) {
            var user = await Context.Client.GetUserAsync(userId);

            if (user == null) {
                await ReplyAsync("Can not find user.");
                return;
            }

            await WhoIsAsync(user);
        }

        [Command]
        public async Task WhoIsAsync(IUser user) {
            var username = $"{user.Username}#{user.DiscriminatorValue}";
            var isHuman = user.IsBot | user.IsWebhook ? "Bot" : "User";
            var guilds = await Context.Client.GetGuildsAsync();
            var locatedOn = 0;

            var embedBuilder = new EmbedBuilder() {
                Title = $"Who is {Format.Bold(username)}?",
                Color = Color.Blue,
                ThumbnailUrl = user.GetAvatarUrl(),
                Footer = new EmbedFooterBuilder() { Text = $"User ID : {user.Id}" }
            };

            foreach (var guild in guilds) if ((await guild.GetUserAsync(user.Id)) != null) locatedOn++;

            embedBuilder.AddField("User Type:", isHuman, true)
                .AddField("Registered:", user.CreatedAt, true)
                .AddField("Located On:", $"{locatedOn} servers", true);
            
            var gUser = await Context.Guild.GetUserAsync(user.Id);

            if (gUser != null) {
                var roles = new StringBuilder();

                foreach (var roleId in gUser.RoleIds) roles.AppendFormat("{0} ", Context.Guild.GetRole(roleId).Mention);

                embedBuilder.AddField("Server Nickname:", gUser.Nickname ?? Format.Bold("N/A"), true)
                    .AddField("Joined Server At:", gUser.JoinedAt.ToString() ?? Format.Bold("Recently"), true)
                    .AddField("Current Server Roles:", roles.ToString() ?? Format.Bold("N/A"))
                    .AddField("ESPer Level:", GetEsperLevel(gUser));
            }

            await ReplyAsync(embed:embedBuilder.Build());
        }
    }
}