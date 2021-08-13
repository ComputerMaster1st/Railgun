using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core;
using System;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Enums;
using TreeDiagram.Models.Server;

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

        public void Load() => _client.UserLeft += (user) =>
        {
            Task.Run(() => ExecuteAsync(user)).ConfigureAwait(false);
            return Task.CompletedTask;
        };

        private Task ExecuteAsync(SocketGuildUser user)
        {
            ServerJoinLeave data;
            string notification;

            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var profile = db.ServerProfiles.GetData(user.Guild.Id);

                if (profile == null) return Task.CompletedTask;

                var inactivityData = profile.Inactivity;

                inactivityData?.Users.RemoveAll(u => u.UserId == user.Id);

                data = profile.JoinLeave;
                var sMention = profile.Globals;
                var userProfile = db.UserProfiles.GetData(user.Id);

                if (data == null) return Task.CompletedTask;

                notification = data.GetMessage(MsgType.Leave);

                if (!string.IsNullOrEmpty(notification)) notification = notification.Replace("<server>", user.Guild.Name).Replace("<user>", user.Username);
                if ((sMention != null && sMention.DisableMentions) || (userProfile != null && userProfile.Globals.DisableMentions)) notification = notification.Replace("<user#disc>", $"{user.Username}#{user.DiscriminatorValue}");
            }

            return SystemUtilities.SendJoinLeaveMessageAsync(data, user, notification, _botLog);
        }
    }
}