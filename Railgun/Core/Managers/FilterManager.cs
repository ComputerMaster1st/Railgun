using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Railgun.Core.Filters;
using TreeDiagram;

namespace Railgun.Core.Managers
{
    public class FilterManager
    {
        private readonly IServiceProvider _services;
        private readonly List<IMessageFilter> _filters = new List<IMessageFilter>();

        public FilterManager(IServiceProvider services)
            => _services = services;

        public void RegisterFilter(IMessageFilter filter) => _filters.Add(filter);

        public async Task<IUserMessage> ApplyFilterAsync(IUserMessage msg) {
            IUserMessage result = null;

            using (var scope = _services.CreateScope()) {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                foreach (var filter in _filters) {
                    result = await filter.FilterAsync(msg, db);

                    if (result != null) break;
                }
            }

            return result;
        }
    }
}