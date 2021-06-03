using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Core.Configuration;
using Railgun.Music;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
		[Alias("join"), BotPerms(GuildPermission.Connect & GuildPermission.Speak)]
        public class MusicJoin : SystemBase
        {
			private readonly MasterConfig _config;
			private readonly PlayerController _players;

			public MusicJoin(MasterConfig config, PlayerController players)
            {
				_config = config;
				_players = players;
            }

			[Command]
			public Task ExecuteAsync()
			{
				if (_players.GetPlayer(Context.Guild.Id) != null)
					return ReplyAsync($"Sorry, I'm already in a voice channel. If you're experiencing problems, please do {Format.Code($"{_config.DiscordConfig.Prefix}music reset stream.")}");

				var user = (IGuildUser)Context.Author;
				var vc = user.VoiceChannel;

				if (vc == null)
					return ReplyAsync("Please go into a voice channel before inviting me.");

				return _players.CreatePlayerAsync(user, vc, (ITextChannel)Context.Channel);
			}
		}
    }
}
