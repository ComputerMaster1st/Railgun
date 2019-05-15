using System.Threading.Tasks;
using Discord;
using Railgun.Core.Configuration;
using Railgun.Core.Containers;
using Railgun.Events;

namespace Railgun.Music.Events
{
    public class ConnectedEvent : IPlayerEvent
    {
        private readonly MasterConfig _config;
        private readonly IDiscordClient _client;
        private PlayerContainer _container;

        public ConnectedEvent(MasterConfig config, IDiscordClient client)
        {
            _config = config;
            _client = client;
        }

        public void Load(PlayerContainer container) 
        {
            _container = container;
            _container.Player.Connected += async (s, a) => await ExecuteAsync();
        }

        private Task ExecuteAsync()
			=> PlayerUtilities.CreateOrModifyMusicPlayerLogEntryAsync(_config, _client, _container);
    }
}