using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Enums;
using TreeDiagram;
using TreeDiagram.Enums;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.SubModels;
using TreeDiagram.Models.User;

namespace Railgun.Events
{
    public class OnUserJoinEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly BotLog _botLog;
        private readonly IServiceProvider _services;

        public OnUserJoinEvent(DiscordShardedClient client, BotLog botLog, IServiceProvider services)
        {
            _client = client;
            _botLog = botLog;
            _services = services;
        }

        public void Load() => _client.UserJoined += (user) => Task.Factory.StartNew(async () => await ExecuteAsync(user));

        private Task ExecuteAsync(SocketGuildUser user)
        {
            ServerJoinLeave data;
            string username;
            string notification;

			using (var scope = _services.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var profile = db.ServerProfiles.GetData(user.Guild.Id);

                if (profile == null) return Task.CompletedTask;

				if (profile.Inactivity.IsEnabled && profile.Inactivity.InactiveDaysThreshold != 0 && 
				    profile.Inactivity.InactiveRoleId != 0)
				{
					if (profile.Inactivity.Users.Any(u => u.UserId == user.Id))
						profile.Inactivity.Users.First(u => u.UserId == user.Id).LastActive = DateTime.Now;
					else profile.Inactivity.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
				}
				
				data = profile.JoinLeave;
                var sMention = profile.Globals;

                var userProfile = db.UserProfiles.GetData(user.Id);
                username = SystemUtilities.GetUsernameOrMention(db, user);

                notification = data.GetMessage(MsgType.Join);

                if (string.IsNullOrEmpty(notification)) return Task.CompletedTask;

                notification = notification.Replace("<server>", user.Guild.Name).Replace("<user>", username);
                if ((sMention != null && sMention.DisableMentions) || (userProfile != null && userProfile.Globals.DisableMentions)) notification = notification.Replace("<user#disc>", $"{username}#{user.DiscriminatorValue}");

			}
            
			return SystemUtilities.SendJoinLeaveMessageAsync(data, user, notification, _botLog);
        }
    }
}