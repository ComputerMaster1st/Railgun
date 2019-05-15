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

			using (var scope = _services.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				var inactivityData = db.ServerInactivities.GetData(user.Guild.Id);

				if (inactivityData != null && inactivityData.IsEnabled && inactivityData.InactiveDaysThreshold != 0 && 
				    inactivityData.InactiveRoleId != 0)
				{
					if (inactivityData.Users.Any(u => u.UserId == user.Id))
						inactivityData.Users.First(u => u.UserId == user.Id).LastActive = DateTime.Now;
					else inactivityData.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
				}
				
				data = db.ServerJoinLeaves.GetData(user.Guild.Id);
                username = SystemUtilities.GetUsernameOrMention(db, user);
			}

			if (data == null) return Task.CompletedTask;

			var notification = data.GetMessage(MsgType.Join);

			if (string.IsNullOrEmpty(notification)) return Task.CompletedTask;

			notification = notification.Replace("<server>", user.Guild.Name).Replace("<user>", username);
			return SystemUtilities.SendJoinLeaveMessageAsync(data, user, notification, _botLog);
        }
    }
}