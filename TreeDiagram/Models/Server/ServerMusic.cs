using System.Collections.Generic;
using MongoDB.Bson;
using TreeDiagram.Models.SubModels;

namespace TreeDiagram.Models.Server
{
	public class ServerMusic : ConfigBase
	{
		public ulong AutoTextChannel { get; set; }
		public ulong AutoVoiceChannel { get; set; }
		public bool AutoSkip { get; set; }
		public bool AutoDownload { get; set; }
		public bool PlaylistAutoLoop { get; set; } = true;
		public ObjectId PlaylistId { get; set; } = ObjectId.Empty;
		public bool VoteSkipEnabled { get; set; }
		public int VoteSkipLimit { get; set; } = 50;
		public bool SilentNowPlaying { get; set; }
		public bool SilentSongProcessing { get; set; }
		public ulong NowPlayingChannel { get; set; }
		public virtual List<UlongRoleId> AllowedRoles { get; private set; } = new List<UlongRoleId>();

		public ServerMusic(ulong id) : base(id) { }
	}
}