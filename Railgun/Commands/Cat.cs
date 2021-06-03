using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Apis.RandomCat;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands
{
    [Alias("cat"), BotPerms(ChannelPermission.AttachFiles)]
    public class Cat : SystemBase
    {
        private readonly RandomCat _randomCat;

        public Cat(RandomCat randomCat)
            => _randomCat = randomCat;
        
        [Command]
        public async Task ExecuteAsync() 
            => await Context.Channel.SendFileAsync(await _randomCat.GetRandomCatAsync(), "CatImg.png");
    }
}