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
        private SystemContext _ctx = null;

        public PrefixPipeline(MasterConfig config, TreeDiagramContext db) {
            _config = config;
            _db = db;
        }

        public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            _ctx = context.Context as SystemContext;
            var msg = (IUserMessage)_ctx.Message;
            var content = msg.Content;
            var sCommand = await _db.ServerCommands.GetAsync(_ctx.Guild.Id);

            if (((sCommand == null || !sCommand.RespondToBots) && msg.Author.IsBot) || msg.Author.IsWebhook) 
                return new PrefixResult();

            var uCommand = await _db.UserCommands.GetAsync(msg.Author.Id);

            if (content.StartsWith(_config.DiscordConfig.Prefix)) 
                return await ValidPrefixExecuteAsync(context, _config.DiscordConfig.Prefix.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else if (content.StartsWith(_ctx.Client.CurrentUser.Mention))
                return await ValidPrefixExecuteAsync(context, _ctx.Client.CurrentUser.Mention.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else if ((sCommand != null && !string.IsNullOrEmpty(sCommand.Prefix)) && content.StartsWith(sCommand.Prefix))
                return await ValidPrefixExecuteAsync(context, sCommand.Prefix.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else if ((uCommand != null && !string.IsNullOrEmpty(uCommand.Prefix)) && content.StartsWith(uCommand.Prefix)) 
                return await ValidPrefixExecuteAsync(context, uCommand.Prefix.Length, sCommand.DeleteCmdAfterUse, msg, next);
            else return new PrefixResult();
        }

        private async Task<IResult> ValidPrefixExecuteAsync(CommandExecutionContext context, int prefixLength, bool deleteCmd, IUserMessage msg, Func<Task<IResult>> next) {
            context.PrefixLength = prefixLength;

            var self = await ((IGuild)_ctx.Guild).GetCurrentUserAsync();
            var perms = self.GetPermissions((IGuildChannel)_ctx.Channel);
            
            if (deleteCmd && perms.ManageMessages) await msg.DeleteAsync();
            
            return await next();
        }
    }
} 