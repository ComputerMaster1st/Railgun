using Railgun.Core.Containers;

namespace Railgun.Music.Events
{
    public interface IPlayerEvent
    {
         void Load(PlayerContainer container);
    }
}