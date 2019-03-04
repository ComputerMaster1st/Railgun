using System;
using System.Threading.Tasks;

namespace Railgun.Core.Containers
{
    public class RemindMeTimerContainer : TimerContainer
    {
        public RemindMeTimerContainer(IServiceProvider services) : base(services)
        {
        }

        protected override void DeleteData()
        {
            throw new NotImplementedException();
        }

        protected override Task RunAsync()
        {
            throw new NotImplementedException();
        }
    }
}
