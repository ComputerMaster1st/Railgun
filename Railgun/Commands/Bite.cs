using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands
{
	[Alias("bite")]
	public class Bite : SystemBase
	{
		private readonly MasterConfig _config;
		private readonly CommandUtils _commandUtils;

		public Bite(MasterConfig config, CommandUtils commandUtils)
		{
			_config = config;
			_commandUtils = commandUtils;
		}

		[Command]
		public async Task BiteAsync(IUser user = null)
		{
			var data = Context.Database.FunBites.GetData(Context.Guild.Id);

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

			var biterName = _commandUtils.GetUsernameOrMention(biter);
			var biteeName = _commandUtils.GetUsernameOrMention(bitee);

			var biteMessage = data.GetBite().Replace("<biter>", Format.Bold(biterName)).Replace("<bitee>", Format.Bold(biteeName));

			await ReplyAsync(biteMessage);
		}

		[Command("add")]
		public async Task AddAsync([Remainder] string msg)
		{
			if (string.IsNullOrWhiteSpace(msg)) {
				await ReplyAsync("You didn't specify a sentence!");

				return;
			}

			var data = Context.Database.FunBites.GetOrCreateData(Context.Guild.Id);

			if (!data.IsEnabled) {
				await ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");

				return;
			}

			data.AddBite(msg);

			await ReplyAsync($"Added Sentence: {Format.Code(msg)}");
		}

		[Command("list"), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ListAsync()
		{
			var data = Context.Database.FunBites.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync($"There are no bite sentences available. Use {Format.Code($"{_config.DiscordConfig.Prefix}bite add <message>")} to add some.");

				return;
			} else if (!data.IsEnabled) {
				await ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");

				return;
			} else if (data.Bites.Count < 1) {
				await ReplyAsync($"There are no bite sentences available. To add a sentence, use {Format.Code($"{_config.DiscordConfig.Prefix}bite add <sentence>")}. You will need to place {Format.Code("<biter>")} & {Format.Code("<bitee>")} in the sentence somewhere to make this work.");

				return;
			}

			var output = new StringBuilder()
				.AppendFormat("List of Bite Sentences: ({0} Total)", data.Bites.Count).AppendLine().AppendLine();

			data.Bites.ForEach(bite => output.AppendFormat("[{0}] {1}", Format.Code(data.Bites.IndexOf(bite).ToString()), bite).AppendLine());

			if (output.Length < 1900) {
				await ReplyAsync(output.ToString());

				return;
			}

			await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "Bites.txt", output.ToString(),
				$"List of {Format.Bold(data.Bites.Count.ToString())} Bite Sentences");
		}

		[Command("remove"), UserPerms(GuildPermission.ManageMessages)]
		public async Task RemoveAsync(int index)
		{
			var data = Context.Database.FunBites.GetData(Context.Guild.Id);

			if (data == null || data.Bites.Count < 1) {
				await ReplyAsync("The list of bite sentences is already empty.");

				return;
			} else if (!data.IsEnabled) {
				await ReplyAsync($"Bite is current {Format.Bold("disabled")} on this server.");

				return;
			} else if (index < 0 || index >= data.Bites.Count) {
				await ReplyAsync("The Message Id provided is out of bounds. Please recheck via Bite List.");

				return;
			}

			data.Bites.RemoveAt(index);

			await ReplyAsync("Message Removed.");
		}

		[Command("allowdeny"), UserPerms(GuildPermission.ManageMessages)]
		public async Task AllowDenyAsync()
		{
			var data = Context.Database.FunBites.GetOrCreateData(Context.Guild.Id);

			data.IsEnabled = !data.IsEnabled;

			await ReplyAsync($"Bites are now {(data.IsEnabled ? Format.Bold("enabled") : Format.Bold("disabled"))}!");
		}

		[Command("reset"), UserPerms(GuildPermission.ManageMessages)]
		public async Task ResetAsync()
		{
			var data = Context.Database.FunBites.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("Bites has no data to reset.");

				return;
			}

			Context.Database.FunBites.Remove(data);

			await ReplyAsync("Bites has been reset.");
		}
	}
}