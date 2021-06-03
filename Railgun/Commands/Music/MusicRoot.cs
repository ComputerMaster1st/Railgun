using Finite.Commands;
using Railgun.Core;
using Railgun.Core.Attributes;
using System;
using System.Threading.Tasks;

namespace Railgun.Commands.Music
{
    public partial class Music
    {
        [Alias("root"), BotAdmin]
        public partial class MusicRoot : SystemBase
        {
            [Command]
            public Task ExecuteAsync()
                => throw new NotImplementedException("This is a module name only. Does not run commands on it's own.");
        }
    }
}