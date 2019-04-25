namespace Railgun.Music.PlayerEventArgs
{
    public class ConnectedEventArgs : PlayerEventArgs
    {
        public ConnectedEventArgs(ulong guildId) : base(guildId) { }
    }
}