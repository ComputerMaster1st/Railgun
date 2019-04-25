using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Railgun.Filters;
using Railgun.Utilities;

namespace Railgun.Events.OnMessageEvents
{
    public class OnFilterSubEvent : IOnMessageSubEvent
    {
        private readonly FilterLoader _filterLoader;
        private readonly Analytics _analytics;

        public OnFilterSubEvent(FilterLoader loader, Analytics analytics)
        {
            _filterLoader = loader;
            _analytics = analytics;
        }

        public Task ExecuteAsync(SocketMessage message)
        {
            var msg = message as IUserMessage;
            var filterMsg = _filterLoader.ApplyFilter(msg);

			if (filterMsg != null) {
				Task.Run(async () => {
					try
                    {
						await msg.DeleteAsync();
						_analytics.FilterDeletedMessages++;
						await Task.Delay(5000);
						await filterMsg.DeleteAsync();
					} 
                    catch { // Ignored
			        }
				}).GetAwaiter();
			}

            return Task.CompletedTask;
        }
    }
}