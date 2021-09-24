using Finite.Commands;
using Railgun.Core;
using System.Threading.Tasks;

namespace Railgun.Commands
{
    [Alias("invite")]
    public class Invite : SystemBase
    {
        [Command]
        public Task ExecuteAsync()
            => ReplyAsync($"Railgun Invite Link: https://discordapp.com/api/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot");
    }
}