using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Core.Containers
{
    public class RemindMeTimerContainer : TimerContainer
    {
        private readonly TimerRemindMe _data;

        public RemindMeTimerContainer(IServiceProvider services, ITreeTimer data) : base(services)
            => _data = (TimerRemindMe)data;

        protected override void DeleteData()
        {
            using (var scope = _services.CreateScope())
            {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerRemindMes.Remove(_data);
            }
        }

        protected override Task RunAsync()
        {
            throw new NotImplementedException();
        }
    }
}
