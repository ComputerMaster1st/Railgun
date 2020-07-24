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
				return await ValidPrefixExecuteAsync(context, _config.DiscordConfig.Prefix, msg, next);
			else if (content.StartsWith(ctx.Client.CurrentUser.Mention, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, ctx.Client.CurrentUser.Mention, msg, next);
			else if ((sCommand != null && !string.IsNullOrEmpty(sCommand.Prefix)) && content.StartsWith(sCommand.Prefix, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, sCommand.Prefix, msg, next);
			else if ((uCommand != null && !string.IsNullOrEmpty(uCommand.Prefix)) && content.StartsWith(uCommand.Prefix, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, uCommand.Prefix, msg, next);
			else return new PrefixResult();
		}

		private async Task<IResult> ValidPrefixExecuteAsync(CommandExecutionContext context, string prefix, IUserMessage msg, Func<Task<IResult>> next)
		{
			if (msg.Content.Length <= prefix.Length) return new PrefixResult();

			int spaces = 0;

			for (var i = prefix.Length; i < msg.Content.Length; i++)
			{
				var c = msg.Content[i];

				if (!char.IsWhiteSpace(c)) break;

				spaces++;
			}

			context.PrefixLength = prefix.Length + spaces;
			return await next();
		}
	}
}