using System;
using System.Text;
using System.Threading.Tasks;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;

namespace Railgun.Commands
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

        private string RandomAnswer {
            get {
                var rand = new Random();
                var result = string.Empty;
                var retry = 5;

                while (string.IsNullOrEmpty(result))
                {
                    try { result = _responses[rand.Next(0, _responses.Length)]; }
                    catch
                    {
                        retry--;
                        if (retry == 0) return "It appears the \"Magic 8 Ball.exe\" has stopped working! Use command again to retry.";
                    }
                }

                return result;
            }
        }

        [Command]
        public Task MagicBallAsync([Remainder] string query = null) => ReplyAsync(new StringBuilder()
                .AppendFormat("{0}8Ball's Response: {1}", string.IsNullOrWhiteSpace(query) ? "" :
                    string.Format("Your Question: {0} {1} ", query, SystemUtilities.GetSeparator, RandomAnswer)).ToString());
    }
}