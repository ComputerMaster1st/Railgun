namespace Railgun.Core.Music.PlayerEventArgs
{
    public class ConnectedPlayerEventArgs : PlayerEventArgs
    {
        public ConnectedPlayerEventArgs(ulong guildId) : base(guildId) { }
    }
}