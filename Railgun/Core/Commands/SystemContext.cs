using System;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using TreeDiagram;

namespace Railgun.Core.Commands
{
    public class SystemContext : CommandContext
    {
        private readonly IServiceProvider _services;
        private TreeDiagramContext _database = null;

        public TreeDiagramContext Database { get {
            if (_database == null) _database = _services.GetService<TreeDiagramContext>();

            return _database;
        }}

        public SystemContext(IDiscordClient client, IUserMessage msg, IServiceProvider services) : base(client, msg)
            => _services = services;
        
        public void DisposeDatabase() {
            if (_database != null) _database.Dispose();
        }
    }
}