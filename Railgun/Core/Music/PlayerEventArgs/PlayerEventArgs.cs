using System;

namespace Railgun.Core.Music.PlayerEventArgs
{
    public abstract class PlayerEventArgs : EventArgs
    {
        public ulong GuildId { get; }

        public PlayerEventArgs(ulong guildId) => GuildId = guildId;
    }
}