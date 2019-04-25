using System;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core;

namespace Railgun.Commands
{
    [Alias("roll")]
    public class Roll : SystemBase
    {        
        [Command]
        public async Task RollAsync(int num1 = 0, int num2 = 100) 
        {
            var name = SystemUtilities.GetUsernameOrMention(Context.Database, (IGuildUser)Context.Author);
            var rand = new Random();
            var rng = num1 > num2 ? rand.Next(num2, num1) : rand.Next(num1, num2);
            
            await ReplyAsync($"{Format.Bold(name)} has rolled {Format.Bold(rng.ToString())}.");
        }
    }
}