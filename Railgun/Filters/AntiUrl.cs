using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using TreeDiagram;
using TreeDiagram.Interfaces;
using TreeDiagram.Models.Filter;

namespace Railgun.Filters
{
    public class AntiUrl : FilterBase, IMessageFilter
	{
		private readonly Regex _regex = new Regex("(http(s)?)://(www.)?", RegexOptions.Compiled);

		private bool CheckContentForUrl(FilterUrl data, string content)
		{
			if (data.DenyMode && data.BannedUrls.Count < 1 && _regex.IsMatch(content)) return true;

			foreach (var url in data.BannedUrls)
			{
				if (data.DenyMode && !content.Contains(url) && _regex.IsMatch(content)) return true;
				if (!data.DenyMode && content.Contains(url)) return true;
			}

			return false;
		}

		public async Task<IUserMessage> FilterAsync(ITextChannel tc, IUserMessage message, TreeDiagramContext context)
		{
			var data = context.FilterUrls.GetData(tc.GuildId);

			if (!CheckConditions(data as ITreeFilter, message)) return null;

			var self = await tc.Guild.GetCurrentUserAsync();
			var user = message.Author;

			if (message.Author.Id == self.Id) return null;
			if (!self.GetPermissions(tc).ManageMessages) 
			{
				await tc.SendMessageAsync($"{Format.Bold("Anti-Url :")} Triggered but missing {Format.Bold("Manage Messages")} permission!");
				return null;
			}

			var content = message.Content.ToLower();
			var output = new StringBuilder()
				.AppendFormat("{0} Deleted {1}'s Message! {2}", Format.Bold("Anti-Url :"), user.Mention, Format.Bold("Reason :"));

			if (_regex.IsMatch(content) && CheckContentForUrl(data, content))
			{
				output.AppendFormat("Unlisted Url Block");
				return await tc.SendMessageAsync(output.ToString());
			}
			if (CheckContentForUrl(data, content))
			{
				output.AppendFormat("Listed Url Block");
				return await tc.SendMessageAsync(output.ToString());
			}

			return null;
		}
	}
}