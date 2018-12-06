using System;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core.Commands;
using Railgun.Core.Commands.Attributes;

namespace Railgun.Commands.Fun
{
    [Alias("8ball")]
    public class MagicBall : SystemBase
    {
        private readonly string[] _responses = {
            "It is certain.",
            "It is decidedly so.",
            "Without a doubt.",
            "Yes definitely.",
            "You may rely on it.",
            "As I see it, yes.",
            "Most likely.",
            "Outlook good.",
            "Yes.",
            "Signs point to yes.",
            "Reply hazy try again.",
            "Ask again later.",
            "Better not tell you now.",
            "Cannot predict now.",
            "Concentrate and ask again.",
            "Don't count on it.",
            "My reply is no.",
            "My sources say no.",
            "Outlook not so good.",
            "Very doubtful."
        };
    
        [Command]
        public Task MagicBallAsync([Remainder] string query = null) {
            var rand = new Random();
            var index = rand.Next(0, (_responses.Length - 1));

            return ReplyAsync($"{(string.IsNullOrWhiteSpace(query) ? "" : $"Your Question: {query} || ")}8Ball's Response: {_responses[index]}");
        }
    }
}