using System.Threading.Tasks;
using Discord.WebSocket;

namespace Railgun.Events.OnMessageEvents
{
    public interface IOnMessageEvent
    {
         Task ExecuteAsync(SocketMessage message);
    }
}