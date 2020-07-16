using System;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Results;
using Railgun.Core.Configuration;
using TreeDiagram;

namespace Railgun.Core.Pipelines
{
	public class PrefixPipeline : IPipeline
	{
		private readonly MasterConfig _config;

		public PrefixPipeline(MasterConfig config) => _config = config;

		public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next)
		{
			var ctx = context.Context as SystemContext;
			var msg = (IUserMessage)ctx.Message;
			var content = msg.Content;
			var profile = ctx.Database.ServerProfiles.GetOrCreateData(ctx.Guild.Id);
            var sCommand = profile.Command;

			if (((sCommand == null || !sCommand.RespondToBots) && msg.Author.IsBot) || msg.Author.IsWebhook)
				return new PrefixResult();

			var userProfile = ctx.Database.UserProfiles.GetOrCreateData(ctx.Guild.Id);
            var uCommand = userProfile.Globals;

			if (content.StartsWith(_config.DiscordConfig.Prefix, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, _config.DiscordConfig.Prefix.Length, msg, next);
			else if (content.StartsWith(ctx.Client.CurrentUser.Mention, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, ctx.Client.CurrentUser.Mention.Length, msg, next);
			else if ((sCommand != null && !string.IsNullOrEmpty(sCommand.Prefix)) && content.StartsWith(sCommand.Prefix, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, sCommand.Prefix.Length, msg, next);
			else if ((uCommand != null && !string.IsNullOrEmpty(uCommand.Prefix)) && content.StartsWith(uCommand.Prefix, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, uCommand.Prefix.Length, msg, next);
			else return new PrefixResult();
		}

		private async Task<IResult> ValidPrefixExecuteAsync(CommandExecutionContext context, int prefixLength, IUserMessage msg, Func<Task<IResult>> next)
		{
			if (msg.Content.Length <= prefixLength) return new PrefixResult();
			context.PrefixLength = prefixLength;
			return await next();
		}
	}
}