using System.Text;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;
using Railgun.Core.Configuration;
using Railgun.Core.Utilities;
using TreeDiagram;

namespace Railgun.Commands.Fun
{
	[Alias("rst")]
	public class Rst : SystemBase
	{
		private readonly MasterConfig _config;

		public Rst(MasterConfig config) => _config = config;

		[Command]
		public async Task RstAsync()
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync($"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.");

				return;
			} else if (!data.IsEnabled) {
				await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

				return;
			}

			var msg = data.GetRst();

			if (string.IsNullOrEmpty(msg))
				msg = $"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.";

			await ReplyAsync(msg);
		}

		[Command("add")]
		public async Task AddAsync([Remainder] string msg)
		{
			if (string.IsNullOrWhiteSpace(msg)) {
				await ReplyAsync("Your message was empty. Please add a message to add.");

				return;
			}

			var data = Context.Database.FunRsts.GetOrCreateData(Context.Guild.Id);

			if (!data.IsEnabled) {
				await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

				return;
			}

			data.AddRst(msg);

			await ReplyAsync($"Added To RST: {Format.Code(msg)}");
		}

		[Command("remove"), UserPerms(GuildPermission.ManageMessages)]
		public async Task RemoveAsync(int index)
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync($"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.");

				return;
			} else if (!data.IsEnabled) {
				await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

				return;
			} else if (index < 0 || index >= data.Rst.Count) {
				await ReplyAsync("The Message Id provided is out of bounds. Please recheck via RST List.");

				return;
			}

			data.Rst.RemoveAt(index);

			await ReplyAsync("Message Removed!");
		}

		[Command("list"), BotPerms(ChannelPermission.AttachFiles)]
		public async Task ListAsync()
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync($"RST is empty! Please add some stuff using {Format.Code($"{_config.DiscordConfig.Prefix}rst add [message]")}.");

				return;
			} else if (!data.IsEnabled) {
				await ReplyAsync($"RST is currently {Format.Bold("disabled")} on this server.");

				return;
			}

			var output = new StringBuilder()
				.AppendLine("Randomly Selected Text List :").AppendLine();

			data.Rst.ForEach(msg => output.AppendFormat("[{0}] {1}", Format.Code(data.Rst.IndexOf(msg).ToString()), msg).AppendLine());

			if (output.Length < 1950) {
				await ReplyAsync(output.ToString());

				return;
			}

			await CommandUtils.SendStringAsFileAsync((ITextChannel)Context.Channel, "RST.txt", output.ToString());
		}

		[Command("allowdeny"), UserPerms(GuildPermission.ManageMessages)]
		public async Task AllowDenyAsync()
		{
			var data = Context.Database.FunRsts.GetOrCreateData(Context.Guild.Id);

			data.IsEnabled = !data.IsEnabled;

			await ReplyAsync($"RST is now {(data.IsEnabled ? Format.Bold("enabled") : Format.Bold("disabled"))}!");
		}

		[Command("reset"), UserPerms(GuildPermission.ManageMessages)]
		public async Task ResetAsync()
		{
			var data = Context.Database.FunRsts.GetData(Context.Guild.Id);

			if (data == null) {
				await ReplyAsync("RST has no data to reset.");

				return;
			}

			Context.Database.FunRsts.Remove(data);

			await ReplyAsync("RST has been reset.");
		}
	}
}