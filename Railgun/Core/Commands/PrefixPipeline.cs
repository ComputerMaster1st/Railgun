using System;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Configuration;
using TreeDiagram;

namespace Railgun.Core.Commands
{
    public class PrefixPipeline : IPipeline
    {
        private readonly MasterConfig _config;
        private readonly TreeDiagramContext _db;

        public PrefixPipeline(MasterConfig config, TreeDiagramContext db) {
            _config = config;
            _db = db;
        }

        public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            var ctx = context.Context as SystemContext;
            var msg = (IUserMessage)ctx.Message;
            var content = msg.Content;
            var sCommand = await _db.ServerCommands.GetAsync(ctx.Guild.Id);

            if (((sCommand == null || !sCommand.RespondToBots) && msg.Author.IsBot) || msg.Author.IsWebhook) 
                return new PrefixResult();

            var uCommand = await _db.UserCommands.GetAsync(msg.Author.Id);

            if (content.StartsWith(_config.DiscordConfig.Prefix)) 
                return await ValidPrefixExecuteAsync(context, _config.DiscordConfig.Prefix.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else if (content.StartsWith(ctx.Client.CurrentUser.Mention))
                return await ValidPrefixExecuteAsync(context, ctx.Client.CurrentUser.Mention.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else if ((sCommand != null && !string.IsNullOrEmpty(sCommand.Prefix)) && content.StartsWith(sCommand.Prefix))
                return await ValidPrefixExecuteAsync(context, sCommand.Prefix.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else if ((uCommand != null && !string.IsNullOrEmpty(uCommand.Prefix)) && content.StartsWith(uCommand.Prefix)) 
                return await ValidPrefixExecuteAsync(context, uCommand.Prefix.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else return new PrefixResult();
        }

        private async Task<IResult> ValidPrefixExecuteAsync(CommandExecutionContext ctx, int prefixLength, bool deleteCmd, IUserMessage msg, Func<Task<IResult>> next) {
            ctx.PrefixLength = prefixLength;
            
            if (deleteCmd) await msg.DeleteAsync();
            
            return await next();
        }
    }
} 