using Discord;
using Discord.WebSocket;
using Finite.Commands;
using TreeDiagram;

namespace Railgun.Utilities
{
    public class EvalUtils
    {
        public readonly DiscordShardedClient Client;
        public readonly IDiscordClient IClient;
        public readonly ICommandContext Context;
        public readonly TreeDiagramContext DbContext;

        public EvalUtils(DiscordShardedClient client, ICommandContext context, TreeDiagramContext dbContext) {
            Client = client;
            IClient = client;
            Context = context;
            DbContext = dbContext;
        }
    }
}