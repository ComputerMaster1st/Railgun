using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using Railgun.Core.Attributes;
using TreeDiagram;
using TreeDiagram.Enums;
using TreeDiagram.Models.Server;

namespace Railgun.Events
{
    [PreInitialize]
    public class OnUserLeftEvent
    {
        private readonly BotLog _botLog;
        private readonly IServiceProvider _services;

        public OnUserLeftEvent(DiscordShardedClient client, BotLog botLog, IServiceProvider services)
        {
            _botLog = botLog;
            _services = services;

            client.UserLeft += (user) => Task.Factory.StartNew(async () => await ExecuteAsync(user));
        }

        private Task ExecuteAsync(SocketGuildUser user)
        {
            ServerJoinLeave data;
            string notification;

            using (var scope = _services.CreateScope())
            {
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var profile = db.ServerProfiles.GetData(user.Guild.Id);

                if (profile == null)
                    return Task.CompletedTask;

                var inactivityData = profile.Inactivity;

				inactivityData?.Users.RemoveAll(u => u.UserId == user.Id);

				data = profile.JoinLeave;

                var sMention = profile.Globals;
                var userProfile = db.UserProfiles.GetData(user.Id);

                if (data == null)
                    return Task.CompletedTask;

                notification = data.GetMessage(MsgType.Leave);

                if (!string.IsNullOrEmpty(notification))
                    notification = notification.Replace("<server>", user.Guild.Name).Replace("<user>", user.Username);

                if ((sMention != null && sMention.DisableMentions) || (userProfile != null && userProfile.Globals.DisableMentions))
                    notification = notification.Replace("<user#disc>", $"{user.Username}#{user.DiscriminatorValue}");
            }

            return SystemUtilities.SendJoinLeaveMessageAsync(data, user, notification, _botLog);
        }
    }
}