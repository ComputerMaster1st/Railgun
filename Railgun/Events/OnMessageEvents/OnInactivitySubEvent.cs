using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TreeDiagram;
using TreeDiagram.Models.SubModels;

namespace Railgun.Events.OnMessageEvents
{
    public class OnInactivitySubEvent : IOnMessageSubEvent
    {
        private readonly IServiceProvider _services;

        public OnInactivitySubEvent(IServiceProvider services) => _services = services;

        public async Task ExecuteAsync(SocketMessage message)
        {
            if (message.Author.IsBot  || message.Author.IsWebhook) return;

			var tc = message.Channel as ITextChannel;

			using (var scope = _services.CreateScope())
			{
				var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
				var data = db.ServerInactivities.GetData(tc.GuildId);
				var guild = tc.Guild;
				var user = await guild.GetUserAsync(message.Author.Id);

				if (data == null) return;
				if (!data.IsEnabled || data.InactiveDaysThreshold == 0 || data.InactiveRoleId == 0) return;
				if (guild.OwnerId == user.Id) return;
				if (data.UserWhitelist.Any((f) => f.UserId == user.Id)) return;
				foreach (var roleId in data.RoleWhitelist) if (user.RoleIds.Contains(roleId.RoleId)) return;

				if (data.Users.Any((f) => f.UserId == user.Id))
				{
					if (user.RoleIds.Contains(data.InactiveRoleId))
						await user.RemoveRoleAsync(guild.GetRole(data.InactiveRoleId));
	                    
					data.Users.First(f => f.UserId == user.Id).LastActive = DateTime.Now;
					return;
				}

				data.Users.Add(new UserActivityContainer(user.Id) { LastActive = DateTime.Now });
			}

        }
    }
}