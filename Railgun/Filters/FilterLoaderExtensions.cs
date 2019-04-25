using System;

namespace Railgun.Filters
{
    public static class FilterLoaderExtensions
    {
        public static FilterLoader AddMessageFilter<TFilter>(this FilterLoader manager) where TFilter : class, IMessageFilter
        {
            manager.AddMessageFilter((TFilter)Activator.CreateInstance(typeof(TFilter)));
            return manager;
        }
    }
}