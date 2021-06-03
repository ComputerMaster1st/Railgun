using System;
using System.Threading.Tasks;
using Discord;
using Finite.Commands;
using Railgun.Core.Results;
using Railgun.Core.Configuration;
using TreeDiagram;
using Discord.WebSocket;

namespace Railgun.Core.Pipelines
{
	public class PrefixPipeline : IPipeline
	{
		private readonly MasterConfig _config;

		public PrefixPipeline(MasterConfig config) => _config = config;

		public async Task<IResult> ExecuteAsync(CommandExecutionContext context, Func<Task<IResult>> next)
		{
			var ctx = context.Context as SystemContext;
			var client = ctx.Client as DiscordShardedClient;
			var msg = (IUserMessage)ctx.Message;
			var content = msg.Content;
			var profile = ctx.Database.ServerProfiles.GetData(ctx.Guild.Id);

			if ((profile != null && !profile.Command.RespondToBots) && (msg.Author.IsBot || msg.Author.IsWebhook))
				return new PrefixResult();

			if (content.StartsWith(_config.DiscordConfig.Prefix, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, _config.DiscordConfig.Prefix, msg, next);

			if (content.StartsWith(client.CurrentUser.Mention, StringComparison.CurrentCultureIgnoreCase))
				return await ValidPrefixExecuteAsync(context, client.CurrentUser.Mention, msg, next);

			if (profile != null && !string.IsNullOrEmpty(profile.Command.Prefix))
				if (content.StartsWith(profile.Command.Prefix, StringComparison.CurrentCultureIgnoreCase))
					return await ValidPrefixExecuteAsync(context, profile.Command.Prefix, msg, next);

			var userProfile = ctx.Database.UserProfiles.GetData(ctx.Author.Id);

			if (userProfile != null && !string.IsNullOrEmpty(userProfile.Globals.Prefix)) 
				if (content.StartsWith(userProfile.Globals.Prefix, StringComparison.CurrentCultureIgnoreCase))
					return await ValidPrefixExecuteAsync(context, userProfile.Globals.Prefix, msg, next);
			
			return new PrefixResult();
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