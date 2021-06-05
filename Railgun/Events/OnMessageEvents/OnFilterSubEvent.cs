using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Filters;
using Railgun.Utilities;

namespace Railgun.Events.OnMessageEvents
{
    public class OnFilterSubEvent : IOnMessageEvent
    {
        private readonly FilterLoader _filterLoader;
        private readonly Analytics _analytics;

        public OnFilterSubEvent(FilterLoader loader, Analytics analytics)
        {
            _filterLoader = loader;
            _analytics = analytics;
        }

        public async Task ExecuteAsync(SocketMessage message)
        {
            var msg = message as IUserMessage;
            var filterMsg = await _filterLoader.ApplyFilterAsync(msg);

			if (filterMsg != null) {
				await Task.Run(async () => {
					try
                    {
						await msg.DeleteAsync();
						_analytics.FilterDeletedMessages++;
						await Task.Delay(5000);
						await filterMsg.DeleteAsync();
					} 
                    catch { // Ignored
			        }
				});
			}
        }
    }
}