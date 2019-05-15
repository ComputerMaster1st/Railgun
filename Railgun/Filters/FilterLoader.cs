using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using TreeDiagram;

namespace Railgun.Filters
{
    public class FilterLoader
    {
        private readonly IServiceProvider _services;
        private readonly List<IMessageFilter> _filters = new List<IMessageFilter>();

        public FilterLoader(IServiceProvider services) => _services = services;

        public void AddMessageFilter(IMessageFilter filter) => _filters.Add(filter);

        public async Task<IUserMessage> ApplyFilterAsync(IUserMessage msg) {
            if (string.IsNullOrWhiteSpace(msg.Content)) return null;

            var tc = (ITextChannel)msg.Channel;
            IUserMessage result = null;

            using (var scope = _services.CreateScope())
            {
                var db = scope.ServiceProvider.GetService<TreeDiagramContext>();

                foreach (var filter in _filters) 
                {
                    result = await filter.FilterAsync(tc, msg, db);
                    if (result != null) break;
                }
            }

            return result;
        }
    }
}