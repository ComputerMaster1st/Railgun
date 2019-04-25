using System;

namespace Railgun.Music.PlayerEventArgs
{
    public abstract class PlayerEventArgs : EventArgs
    {
        public ulong GuildId { get; }

        public PlayerEventArgs(ulong guildId) => GuildId = guildId;
    }
}