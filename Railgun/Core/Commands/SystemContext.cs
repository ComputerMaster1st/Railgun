using System;
using Discord;
using Discord.WebSocket;
using Finite.Commands;

namespace Railgun.Core.Commands
{
    public class SystemContext : ICommandContext
    {
        public IDiscordClient Client { get; }
        public SocketMessage Message { get; }
        public ISocketMessageChannel Channel { get; }
        public IUser Author { get; }
        public IGuild Guild { get; }
        public bool IsPrivate => Channel is IPrivateChannel;

        public SystemContext(IDiscordClient client, SocketMessage message) {
            Client = client;
            Message = message;
            Channel = message.Channel;
            Author = message.Author;
            Guild = (Channel as SocketGuildChannel)?.Guild;
        }

        string ICommandContext.Message => Message.Content;

        string ICommandContext.Author => Author.ToString();
    }
}