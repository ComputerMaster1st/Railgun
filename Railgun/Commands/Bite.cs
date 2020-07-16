using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Extensions;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.Fun;

namespace Railgun.Commands
{
    [Alias("bite")]
	public class Bite : SystemBase
	{
		private readonly MasterConfig _config;

		public Bite(MasterConfig config) =>_config = config;

		private FunBite GetData(ulong guildId, bool create = false)
		{
			ServerProfile data;

			if (create)
				data = Context.Database.ServerProfiles.GetOrCreateData(guildId);
			else {
				data = Context.Database.ServerProfiles.GetData(guildId);

				if (data == null) 
					return null;
			}

			if (data.Fun.Bites == null)
				if (create)
					data.Fun.Bites = new FunBite();
			
			return data.Fun.Bites;
		}

		[Command]
		public async Task BiteAsync(IUser user = null)
		{
			var data = GetData(Context.Guild.Id);

			if (data == null || data.Bites.Count < 1) {
				var output = new StringBuilder()
					.AppendLine("There are no bite sentences available. Please ask an admin to set some up.")
					.AppendFormat("Admin, to add a sentence, use {0}. You will need to place `{1} & {2} in the sentence somewhere to make this work.", Format.Code($"{_config.DiscordConfig.Prefix}bite add <sentence>"), Format.Code("<biter>"), Format.Code("<bitee>"));

				await ReplyAsync(output.ToString());

				return;
			} else if (!data.IsEnabled) {
				await ReplyAsync($"Bite is currently {Format.Bold("disabled")} on this server.");
				return;
			}

			var rand = new Random();
			var i = 0;

			if (user == null) i = rand.Next(1, 2);
			else i = rand.Next(3, 6);

			IGuildUser bitee = null;
			IGuildUser biter = null;

			switch (i) {
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

			var biterName = SystemUtilities.GetUsernameOrMention(Context.Database, biter);
			var biteeName = SystemUtilities.GetUsernameOrMention(Context.Database, bitee);
			var biteMessage = data.GetBite().Replace("<biter>", Format.Bold(biterName)).Replace("<bitee>", Format.Bold(biteeName));

			await ReplyAsync(biteMessage);
		}

		[Command("add")]
		public Task AddAsync([Remainder] string msg)
		{
			if (string.IsNullOrWhiteSpace(msg))
				return ReplyAsync("You didn't specify a sentence!");

			var data = GetData(Context.Guild.Id, true);

			if (!data.IsEnabled)
				return ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");

			data.AddBite(msg);
			return ReplyAsync($"Added Sentence: {Format.Code(msg)}");
		}

		[Command("list"), BotPerms(ChannelPermission.AttachFiles)]
		public Task ListAsync()
		{
			var data = GetData(Context.Guild.Id);

			if (data == null)
				return ReplyAsync($"There are no bite sentences available. Use {Format.Code($"{_config.DiscordConfig.Prefix}bite add <message>")} to add some.");
			if (!data.IsEnabled)
				return ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");
			if (data.Bites.Count < 1)
				return ReplyAsync($"There are no bite sentences available. To add a sentence, use {Format.Code($"{_config.DiscordConfig.Prefix}bite add <sentence>")}. You will need to place {Format.Code("<biter>")} & {Format.Code("<bitee>")} in the sentence somewhere to make this work.");

			var output = new StringBuilder()
				.AppendFormat("List of Bite Sentences: ({0} Total)", data.Bites.Count).AppendLine().AppendLine();

			data.Bites.ForEach(bite => output.AppendFormat("[{0}] {1}", Format.Code(data.Bites.IndexOf(bite).ToString()), bite).AppendLine());

			if (output.Length < 1900)
				return ReplyAsync(output.ToString());

			return (Context.Channel as ITextChannel).SendStringAsFileAsync("Bites.txt", output.ToString(),
				$"List of {Format.Bold(data.Bites.Count.ToString())} Bite Sentences");
		}

		[Command("remove"), UserPerms(GuildPermission.ManageMessages)]
		public Task RemoveAsync(int index)
		{
			var data = GetData(Context.Guild.Id);

			if (data == null || data.Bites.Count < 1)
				return ReplyAsync("The list of bite sentences is already empty.");
			if (!data.IsEnabled)
				return ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");
			if (index < 0 || index >= data.Bites.Count)
				return ReplyAsync("The Message Id provided is out of bounds. Please recheck via Bite List.");

			data.Bites.RemoveAt(index);
			return ReplyAsync("Message Removed.");
		}

		[Command("allowdeny"), UserPerms(GuildPermission.ManageMessages)]
		public Task AllowDenyAsync()
		{
			var data = GetData(Context.Guild.Id, true);
			data.IsEnabled = !data.IsEnabled;
			return ReplyAsync($"Bites are now {(data.IsEnabled ? Format.Bold("enabled") : Format.Bold("disabled"))}!");
		}

		[Command("reset"), UserPerms(GuildPermission.ManageMessages)]
		public Task ResetAsync()
		{
			var data = Context.Database.ServerProfiles.GetData(Context.Guild.Id);

			if (data == null || data.Fun.Bites == null)
				return ReplyAsync("Bites has no data to reset.");

			data.Fun.Bites = null;
			return ReplyAsync("Bites has been reset.");
		}
	}
}