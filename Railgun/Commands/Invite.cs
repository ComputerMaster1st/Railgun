using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("invite")]
    public class Invite : SystemBase
    {
        [Command]
        public Task ExecuteAsync()
            => ReplyAsync("Railgun Invite Link: https://discordapp.com/api/oauth2/authorize?client_id=261878358625746964&permissions=3271687&scope=bot");
    }
}