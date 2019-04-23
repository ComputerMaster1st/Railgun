using System;
using Railgun.Core.Filters;
using Railgun.Core.Managers;

namespace Railgun.Core.Extensions
{
    public static class FilterManagerExtensions 
    {
        public static FilterManager AddMessageFilter<TFilter>(this FilterManager manager) where TFilter : class, IMessageFilter
        {
            manager.AddMessageFilter((TFilter)Activator.CreateInstance(typeof(TFilter)));
            return manager;
        }
    }
}