using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("master")]
        public class RootMaster : SystemBase
        {
            private readonly MasterConfig _config;

            public RootMaster(MasterConfig config)
                => _config = config;

            [Command]
            public Task ExecuteAsync()
            {
                _config.AssignMasterGuild(Context.Guild.Id);

                return ReplyAsync($"This server {Format.Bold(Context.Guild.Name)} has been set as master.");
            }
        }
    }
}
