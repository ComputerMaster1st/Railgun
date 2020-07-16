using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;

namespace TreeDiagram.Models.Server
{
	public class ServerMusic
	{
		public ulong AutoTextChannel { get; set; }
		public ulong AutoVoiceChannel { get; set; }
		public bool AutoSkip { get; set; }
		public bool AutoDownload { get; set; }
		public string AutoPlaySong { get; set; } = string.Empty;
		public bool PlaylistAutoLoop { get; set; } = true;
		public ObjectId PlaylistId { get; set; } = ObjectId.Empty;
		public bool VoteSkipEnabled { get; set; }
		public int VoteSkipLimit { get; set; } = 50;
		public bool SilentNowPlaying { get; set; }
		public bool SilentSongProcessing { get; set; }
		public ulong NowPlayingChannel { get; set; }
		[Column(TypeName="jsonb")]
		public virtual List<ulong> AllowedRoles { get; private set; } = new List<ulong>();
		public bool WhitelistMode { get; set; }
	}
}