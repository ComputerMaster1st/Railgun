using System;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Commands.Results;
using Railgun.Core.Configuration;
using TreeDiagram;

namespace Railgun.Core.Commands.Pipelines
{
    public class PrefixPipeline : IPipeline
    {
        private readonly MasterConfig _config;
        private SystemContext _ctx = null;

        public PrefixPipeline(MasterConfig config) => _config = config;

        public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            _ctx = context.Context as SystemContext;
            var msg = (IUserMessage)_ctx.Message;
            var content = msg.Content;
            var sCommand = await _ctx.Database.ServerCommands.GetAsync(_ctx.Guild.Id);

            if (((sCommand == null || !sCommand.RespondToBots) && msg.Author.IsBot) || msg.Author.IsWebhook) 
                return new PrefixResult();

            var uCommand = await _ctx.Database.UserCommands.GetAsync(msg.Author.Id);

            if (content.StartsWith(_config.DiscordConfig.Prefix)) 
                return await ValidPrefixExecuteAsync(context, _config.DiscordConfig.Prefix.Length, msg, next);
            else if (content.StartsWith(_ctx.Client.CurrentUser.Mention))
                return await ValidPrefixExecuteAsync(context, _ctx.Client.CurrentUser.Mention.Length, msg, next);
            else if ((sCommand != null && !string.IsNullOrEmpty(sCommand.Prefix)) && content.StartsWith(sCommand.Prefix))
                return await ValidPrefixExecuteAsync(context, sCommand.Prefix.Length, msg, next);
            else if ((uCommand != null && !string.IsNullOrEmpty(uCommand.Prefix)) && content.StartsWith(uCommand.Prefix)) 
                return await ValidPrefixExecuteAsync(context, uCommand.Prefix.Length, msg, next);
            else return new PrefixResult();
        }

        private async Task<IResult> ValidPrefixExecuteAsync(CommandExecutionContext context, int prefixLength, IUserMessage msg, Func<Task<IResult>> next) {
            if (msg.Content.Length <= prefixLength) return new PrefixResult();

            context.PrefixLength = prefixLength;
            
            return await next();
        }
    }
} 