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
    public class OnUserLeftEvent : IEvent
    {
        private readonly DiscordShardedClient _client;
        private readonly BotLog _botLog;
        private readonly IServiceProvider _services;

        public OnUserLeftEvent(DiscordShardedClient client, BotLog botLog, IServiceProvider services)
        {
            _client = client;
            _botLog = botLog;
            _services = services;
        }

        public void Load() => _client.UserLeft += (user) => Task.Factory.StartNew(async () => await ExecuteAsync(user));

        private Task ExecuteAsync(SocketGuildUser user)
        {
            ServerJoinLeave data;
            ServerMention sMention;
            UserMention uMention;

            using (var scope = _services.CreateScope())
            {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				var inactivityData = db.ServerInactivities.GetData(user.Guild.Id);

				inactivityData?.Users.RemoveAll(u => u.UserId == user.Id);

				data = db.ServerJoinLeaves.GetData(user.Guild.Id);
                sMention = db.ServerMentions.GetData(user.Guild.Id);
                uMention = db.UserMentions.GetData(user.Id);
            }

			if (data == null) return Task.CompletedTask;

			var notification = data.GetMessage(MsgType.Leave);

			if (!string.IsNullOrEmpty(notification)) notification = notification.Replace("<server>", user.Guild.Name).Replace("<user>", user.Username);
            if (sMention.DisableMentions || (uMention != null && uMention.DisableMentions)) notification.Replace("<user#disc>", $"{user.Username}#{user.DiscriminatorValue}");

            return SystemUtilities.SendJoinLeaveMessageAsync(data, user, notification, _botLog);
        }
    }
}