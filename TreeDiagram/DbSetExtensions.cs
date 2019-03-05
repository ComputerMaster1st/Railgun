using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TreeDiagram.Models;

namespace TreeDiagram
{
    public static class DbSetExtensions
	{
		public static TEntity CreateTimer<TEntity>(this DbSet<TEntity> set, ulong guildId, ulong tc, 
			DateTime timerExpire) where TEntity : class, ITreeTimer
		{
			var data = (TEntity)Activator.CreateInstance(typeof(TEntity), guildId, tc, timerExpire);
			set.Add(data);

			return data;
		}

		public static TEntity GetOrCreateData<TEntity>(this DbSet<TEntity> set, ulong id) where TEntity : class, ITreeModel
		{
			var data = set.FirstOrDefault(find => find.Id == id);

			if (data != null) return data;

			data = (TEntity)Activator.CreateInstance(typeof(TEntity), id);
			set.Add(data);

			return data;
		}

		public static TEntity GetData<TEntity>(this DbSet<TEntity> set, ulong id) where TEntity : class, ITreeModel
			=> set.FirstOrDefault(find => find.Id == id);
	}
}