using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands;

namespace Railgun.Commands
{
    [Alias("invite")]
    public class Invite : SystemBase
    {
        [Command]
        public Task InviteAsync()
            => ReplyAsync("Railgun Invite Link: https://discordapp.com/api/oauth2/authorize?client_id=261878358625746964&permissions=3271687&scope=bot");
    }
}