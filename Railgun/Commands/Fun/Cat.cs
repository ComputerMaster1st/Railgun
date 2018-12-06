using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Api;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;

namespace Railgun.Commands.Fun
{
    [Alias("cat")]
    public class Cat : SystemBase
    {
        private readonly RandomCat _randomCat;

        public Cat(RandomCat randomCat) => _randomCat = randomCat;
        
        [Command, BotPerms(ChannelPermission.AttachFiles)]
        public async Task CatAsync() 
            => await Context.Channel.SendFileAsync(await _randomCat.GetRandomCatAsync(), "CatImg.png");
    }
}