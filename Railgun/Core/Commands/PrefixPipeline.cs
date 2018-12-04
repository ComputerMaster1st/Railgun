using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Finite.Commands;
using Finite.Commands;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Configuration;
using TreeDiagram;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.User;

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
            var ctx = context.Context as SocketCommandContext;
            var msg = (IUserMessage)ctx.Message;
            var content = msg.Content;
            var sCommand = await _db.ServerCommands.GetAsync(ctx.Guild.Id);

            if (((sCommand == null || !sCommand.RespondToBots) && msg.Author.IsBot) || msg.Author.IsWebhook) 
                return new PrefixResult();

            UserCommand uCommand = await _db.UserCommands.GetAsync(msg.Author.Id);

            if (content.StartsWith(_config.DiscordConfig.Prefix)) {
                context.PrefixLength = _config.DiscordConfig.Prefix.Length;
                return await next();
            } else if (content.StartsWith(ctx.Client.CurrentUser.Mention)) {
                context.PrefixLength = ctx.Client.CurrentUser.Mention.Length;
                return await next();
            } else if ((sCommand != null && !string.IsNullOrEmpty(sCommand.Prefix)) && content.StartsWith(sCommand.Prefix)) {
                context.PrefixLength = sCommand.Prefix.Length;
                return await next();
            } else if ((uCommand != null && !string.IsNullOrEmpty(uCommand.Prefix)) && content.StartsWith(uCommand.Prefix)) {
                context.PrefixLength = uCommand.Prefix.Length;
                return await next();
            } else return new PrefixResult();
        }
    }
} 