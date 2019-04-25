using System.Collections.Generic;
using Railgun.Core.Containers;
using Railgun.Events;
using Railgun.Music.Events;

namespace Railgun.Music
{
    public class PlayerEventLoader
    {
        private readonly PlayerContainer _container;
        private readonly List<IPlayerEvent> _events = new List<IPlayerEvent>();

        public PlayerEventLoader(PlayerContainer container) => _container = container;

        public PlayerEventLoader LoadEvent(IPlayerEvent playerEvent)
        {
            playerEvent.Load(_container);
            _events.Add(playerEvent);
            return this;
        }
    }
}