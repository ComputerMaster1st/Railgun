using Discord;
using Finite.Commands;
using Railgun.Core;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands
{
    [Alias("roll")]
    public class Roll : SystemBase
    {
        [Command]
        public Task ExecuteAsync(int num1)
        {
            var name = SystemUtilities.GetUsernameOrMention(Context.Database, Context.Author as IGuildUser);
            var rand = new Random();
            var rng = rand.Next(0, num1);

            return ReplyAsync(string.Format("{0} has rolled {1}.",
                Format.Bold(name),
                Format.Bold(rng.ToString())));
        }

        [Command]
        public Task ExecuteAsync()
            => ExecuteAsync(100);
    }
}