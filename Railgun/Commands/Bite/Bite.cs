using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Configuration;
using TreeDiagram;

namespace Railgun.Commands.Bite
{
    [Alias("bite")]
	public partial class Bite : SystemBase
	{
		private readonly MasterConfig _config;

		public Bite(MasterConfig config)
			=>_config = config;

		[Command]
		public async Task ExecuteAsync(IUser user)
		{
			var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
            var data = profile.Fun.Bites;

			if (data.Bites.Count < 1) {
				var output = new StringBuilder()
					.AppendLine("There are no bite sentences available. Please ask an admin to set some up.")
					.AppendFormat("Admin, to add a sentence, use {0}. You will need to place `{1} & {2} in the sentence somewhere to make this work.", Format.Code($"{_config.DiscordConfig.Prefix}bite add <sentence>"), Format.Code("<biter>"), Format.Code("<bitee>"));

				await ReplyAsync(output.ToString());

				return;
			} 
			if (!data.IsEnabled) {
				await ReplyAsync($"Bite is currently {Format.Bold("disabled")} on this server.");

				return;
			}

			IGuildUser bitee = null;
			IGuildUser biter = null;

			if (data.NoNameRandomness)
            {
				bitee = (IGuildUser)(user ?? Context.Author);
				biter = Context.Author as IGuildUser;
			}
			else
            {
				var rand = new Random();
                int i;

                if (user == null) 
					i = rand.Next(1, 2);
				else 
					i = rand.Next(3, 6);

				switch (i)
				{
					case 1:
						biter = await Context.Guild.GetCurrentUserAsync();
						bitee = await Context.Guild.GetUserAsync(Context.Author.Id);
						break;
					case 2:
						biter = await Context.Guild.GetUserAsync(Context.Author.Id);
						bitee = await Context.Guild.GetCurrentUserAsync();
						break;
					case 3:
						biter = await Context.Guild.GetUserAsync(user.Id);
						bitee = await Context.Guild.GetCurrentUserAsync();
						break;
					case 4:
						biter = await Context.Guild.GetCurrentUserAsync();
						bitee = await Context.Guild.GetUserAsync(user.Id);
						break;
					case 5:
						biter = await Context.Guild.GetUserAsync(user.Id);
						bitee = await Context.Guild.GetUserAsync(Context.Author.Id);
						break;
					case 6:
						biter = await Context.Guild.GetUserAsync(Context.Author.Id);
						bitee = await Context.Guild.GetUserAsync(user.Id);
						break;
				}
			}

			var biterName = SystemUtilities.GetUsernameOrMention(Context.Database, biter);
			var biteeName = SystemUtilities.GetUsernameOrMention(Context.Database, bitee);

			var biteMessage = data.GetBite().Replace("<biter>", Format.Bold(biterName)).Replace("<bitee>", Format.Bold(biteeName));

			await ReplyAsync(biteMessage);
		}

		[Command]
		public Task ExecuteAsync() => ExecuteAsync(null);
	}
}