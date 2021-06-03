using Discord;
using Discord.WebSocket;
using Finite.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Extensions;
using Railgun.Utilities;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Root
{
    public partial class Root
    {
        [Alias("eval")]
        public class RootEval : SystemBase
        {
            [Command]
            public async Task ExecuteAsync([Remainder] string code)
            {
                var eval = new EvalUtils(Context.Client as DiscordShardedClient, Context, Context.Database);
                string output;

                try 
                { 
                    output = (await CSharpScript.EvaluateAsync(code, globals: eval)).ToString();
                }
                catch (Exception ex) 
                { 
                    output = ex.Message; 
                }

                if (output.Length > 1900)
                {
                    await (Context.Channel as ITextChannel).SendStringAsFileAsync("evalresult.txt", output, "Evaluation Results!", false);

                    return;
                }

                await ReplyAsync(Format.Code(output));
            }
        }
    }
}
