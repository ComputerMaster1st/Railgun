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
        public Task ExecuteAsync(int num1, int num2) 
        {
            var name = SystemUtilities.GetUsernameOrMention(Context.Database, Context.Author as IGuildUser);
            var rand = new Random();
            var rng = num1 > num2 ? rand.Next(num2, num1) : rand.Next(num1, num2);
            
            return ReplyAsync($"{Format.Bold(name)} has rolled {Format.Bold(rng.ToString())}.");
        }

        [Command]
        public Task ExecuteAsync(int num1)
            => ExecuteAsync(0, num1);

        [Command]
        public Task ExecuteAsync()
            => ExecuteAsync(0, 100);
    }
}