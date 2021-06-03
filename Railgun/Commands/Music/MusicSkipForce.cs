using Discord;
using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using Railgun.Music;
using System.Threading.Tasks;
using TreeDiagram;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        public partial class MusicSkip
        {
			[Alias("force"), UserPerms(GuildPermission.ManageMessages)]
            public class MusicSkipForce : SystemBase
            {
				private readonly PlayerController _players;

				public MusicSkipForce(PlayerController playerManager)
					=> _players = playerManager;

				[Command]
				public Task ExecuteAsync()
				{
					var profile = Context.Database.ServerProfiles.GetOrCreateData(Context.Guild.Id);
					var data = profile.Music;
					var container = _players.GetPlayer(Context.Guild.Id);

					if (!data.VoteSkipEnabled)
						return ReplyAsync("This command is not available due to Music Vote-Skip being disabled.");

					if (container == null)
						return ReplyAsync("Can not skip current song because I am not in voice channel.");

					container.Player.SkipMusic();

					return ReplyAsync("Force-Skipping music now...");
				}
			}
        }
    }
}
