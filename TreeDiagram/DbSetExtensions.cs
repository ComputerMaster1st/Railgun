using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TreeDiagram.Interfaces;
using TreeDiagram.Models;

namespace TreeDiagram
{
    public static class DbSetExtensions
	{
		public static TEntity CreateTimer<TEntity>(this DbSet<TEntity> set, ulong guildId, DateTime timerExpire) 
			where TEntity : class, ITreeTimer
		{
			var data = (TEntity)Activator.CreateInstance(typeof(TEntity), guildId, timerExpire);
			set.Add(data);

			return data;
		}

		public static TEntity GetOrCreateData<TEntity>(this DbSet<TEntity> set, ulong guildId) where TEntity : class, ITreeModel
		{
			var data = set.FirstOrDefault(find => find.Id == guildId);

			if (data != null) return data;

			data = (TEntity)Activator.CreateInstance(typeof(TEntity), guildId);
			set.Add(data);

			return data;
		}

		public static TEntity GetData<TEntity>(this DbSet<TEntity> set, ulong guildId) where TEntity : class, ITreeModel
			=> set.FirstOrDefault(find => find.Id == guildId);
		
		public static int ConfigCheck<TEntity>(this DbSet<TEntity> set, IEnumerable<ulong> ids) where TEntity : class, ITreeModel
		{
			var badIds = new List<ulong>();
			foreach (var config in set) if (!ids.Contains(config.Id)) badIds.Add(config.Id);
			foreach (var id in badIds) set.DeleteData(id);
			return badIds.Count;
		}

		internal static void DeleteData<TEntity>(this DbSet<TEntity> set, ulong guildId) where TEntity : class, ITreeModel
		{
			var data = set.GetData(guildId);
			if (data != null) set.Remove(data);
		}
	}
}