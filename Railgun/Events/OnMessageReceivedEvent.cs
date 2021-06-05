using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Attributes;
using Railgun.Events.OnMessageEvents;
using Railgun.Utilities;
using TreeDiagram;

namespace Railgun.Events
{
    [PreInitialize]
    public class OnMessageReceivedEvent
    {
        private readonly Analytics _analytics;
        private readonly IServiceProvider _services;
        private readonly List<IOnMessageEvent> _subEvents = new List<IOnMessageEvent>();

        public OnMessageReceivedEvent(DiscordShardedClient client, Analytics analytics, IServiceProvider services)
        {
            _analytics = analytics;
            _services = services;

            client.MessageReceived += (message) => Task.Factory.StartNew(async () => await ExecuteReceivedAsync(message));
            client.MessageUpdated += (cachedMessage, newMessage, channel) => Task.Factory.StartNew(async () => await ExecuteUpdatedAsync(newMessage));
        }

        public void AddSubEvent(IOnMessageEvent sEvent)
            => _subEvents.Add(sEvent);

        private Task ExecuteReceivedAsync(SocketMessage message)
        {
            _analytics.ReceivedMessages++;

            return ExecuteAsync(message);
        }

        private Task ExecuteUpdatedAsync(SocketMessage newMsg)
        {
            _analytics.UpdatedMessages++;

            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                var channel = newMsg.Channel as ITextChannel;
                var profile = db.ServerProfiles.GetData(channel.GuildId);

                if (profile != null && profile.Command.IgnoreModifiedMessages)
                    return Task.CompletedTask;
            }

            return ExecuteAsync(newMsg);          
        }

        private async Task ExecuteAsync(SocketMessage message)
        {
            if (!(message is SocketUserMessage) || !(message.Channel is SocketGuildChannel) || string.IsNullOrEmpty(message.Content)) return;

            foreach (var subEvent in _subEvents)
                await subEvent.ExecuteAsync(message);
        }
    }
}