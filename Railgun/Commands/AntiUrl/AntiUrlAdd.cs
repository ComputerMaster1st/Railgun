using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.AntiUrl
{
    public partial class AntiUrl
    {
		[Alias("add")]
        public class AntiUrlAdd : SystemBase
        {
			[Command]
			public Task ExecuteAsync(string url)
			{
				var newUrl = ProcessUrl(url);
				var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
				var data = profile.Filters.Urls;

				if (data.BannedUrls.Contains(newUrl))
					return ReplyAsync("The Url specified is already listed.");

				data.BannedUrls.Add(newUrl);

				if (!data.IsEnabled) 
					data.IsEnabled = true;

				return ReplyAsync($"The Url {Format.Bold(newUrl)} is now added to list.");
			}
		}
    }
}
