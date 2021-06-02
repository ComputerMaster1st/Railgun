using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Railgun.Commands.Server
{
    public partial class Server
    {
		[Alias("clear"), UserPerms(GuildPermission.ManageMessages), BotPerms(ChannelPermission.ManageMessages), BotPerms(GuildPermission.ReadMessageHistory)]
        public class ServerClear : SystemBase
        {
			[Command]
			public async Task ExecuteAsync(int count)
			{
				var deleted = 0;

				while (count > 0)
				{
					var subCount = count >= 100 ? 100 : count;
					var tc = Context.Channel as ITextChannel;
					var msgs = await tc.GetMessagesAsync(subCount).FlattenAsync();
					var msgsToDelete = new List<IMessage>();

					foreach (var msg in msgs)
						if (msg.CreatedAt > DateTime.Now.AddDays(-13).AddHours(-23).AddMinutes(-50))
							msgsToDelete.Add(msg);

					await tc.DeleteMessagesAsync(msgsToDelete);

					deleted += msgsToDelete.Count;

					if (msgsToDelete.Count() == subCount) count -= subCount;
					else break;
				}

				await ReplyAsync($"Up to {Format.Bold(deleted.ToString())} messages have been deleted from the channel.");
			}

			[Command]
			public Task ExecuteAsync()
				=> ExecuteAsync(100);
		}
    }
}
