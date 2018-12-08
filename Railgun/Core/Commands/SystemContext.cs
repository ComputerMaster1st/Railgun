using System;
using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Microsoft.Extensions.DependencyInjection;
using TreeDiagram;

namespace Railgun.Core.Commands
{
    public class SystemContext : ICommandContext, IDisposable
    {
        private IServiceProvider _services;
        private TreeDiagramContext _database = null;

        public IDiscordClient Client { get; }
        public SocketMessage Message { get; }
        public ISocketMessageChannel Channel { get; }
        public IUser Author { get; }
        public IGuild Guild { get; }
        public bool IsPrivate => Channel is IPrivateChannel;

        public TreeDiagramContext Database { get {
            if (_database == null) _database = _services.GetService<TreeDiagramContext>();

            return _database;
        }}

        public SystemContext(IDiscordClient client, SocketMessage message, IServiceProvider services) {
            _services = services;

            Client = client;
            Message = message;
            Channel = message.Channel;
            Author = message.Author;
            Guild = (Channel as SocketGuildChannel)?.Guild;
        }

        string ICommandContext.Message => Message.Content;

        string ICommandContext.Author => Author.ToString();

        public void Dispose() {
            if (_database != null) _database.Dispose();
        }
    }
}