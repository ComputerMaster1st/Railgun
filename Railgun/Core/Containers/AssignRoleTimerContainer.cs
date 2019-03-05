using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TreeDiagram;
using TreeDiagram.Models;
using TreeDiagram.Models.TreeTimer;

namespace Railgun.Core.Containers {
    
    public class AssignRoleTimerContainer : TimerContainer {
        private readonly TimerAssignRole _data;
        
        public override ITreeTimer Data => _data;

        public AssignRoleTimerContainer(IServiceProvider services, ITreeTimer data) : base(services)
            => _data = (TimerAssignRole)data;
        
        protected override void DeleteData() {
            using (var scope = Services.CreateScope()) {
                scope.ServiceProvider.GetService<TreeDiagramContext>().TimerAssignRoles.Remove(_data);
            }
        }
        
        protected override Task RunAsync() {
            throw new NotImplementedException();
        }
    }
}