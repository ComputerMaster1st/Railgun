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
        public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next) {
            var config = context.ServiceProvider.GetService<MasterConfig>();
            var ctx = context.Context as SocketCommandContext;
            var msg = (IUserMessage)ctx.Message;
            var content = msg.Content;
            ServerCommand sCommand;
            UserCommand uCommand;

            using (var scope = context.ServiceProvider.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();
                sCommand = await db.ServerCommands.GetAsync(ctx.Guild.Id);

                if (((sCommand == null || !sCommand.RespondToBots) && msg.Author.IsBot) || msg.Author.IsWebhook) 
                    return new PrefixResult();

                uCommand = await db.UserCommands.GetAsync(msg.Author.Id);
            }

            if (content.StartsWith(config.DiscordConfig.Prefix)) {
                context.PrefixLength = config.DiscordConfig.Prefix.Length;
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