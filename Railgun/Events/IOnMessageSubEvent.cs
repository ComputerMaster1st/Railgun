using System.Threading.Tasks;
using Discord.WebSocket;

namespace Railgun.Events
{
    public interface IOnMessageSubEvent
    {
         Task ExecuteAsync(SocketMessage message);
    }
}