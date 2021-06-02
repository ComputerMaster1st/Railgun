using Discord;
using Finite.Commands;
using Railgun.Core;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("avatar")]
        public class RootAvatar : SystemBase
        {
            [Command]
            public async Task ExecuteAsync(string url)
            {
                var imageUrl = url ?? Context.Message.Attachments.FirstOrDefault().Url;

                using (var webclient = new HttpClient())
                {
                    var imageStream = await webclient.GetStreamAsync(imageUrl);

                    await Context.Client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(imageStream));

                    await ReplyAsync("Applied Avatar!");
                }
            }

            [Command]
            public Task ExecuteAsync()
            {
                if (Context.Message.Attachments.Count > 0)
                    return ExecuteAsync(null);

                return ReplyAsync("Please specify a url or upload an image.");
            }
        }
    }
}
