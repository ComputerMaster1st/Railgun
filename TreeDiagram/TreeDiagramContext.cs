using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;
using TreeDiagram.Models;
using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.TreeTimer;

namespace TreeDiagram
{
	public sealed class TreeDiagramContext : DbContext
	{
		public DbSet<ServerProfile> ServerProfiles { get; internal set; } = null;
		public DbSet<UserProfile> UserProfiles { get; internal set; } = null;

		public DbSet<TimerAssignRole> TimerAssignRoles { get; internal set; } = null;
		public DbSet<TimerKickUser> TimerKickUsers { get; internal set; } = null;
		public DbSet<TimerRemindMe> TimerRemindMes { get; internal set; } = null;

		public TreeDiagramContext(DbContextOptions optionsBuilder) : base(optionsBuilder) { }    

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			var ulongConverter = new ValueConverter<List<ulong>, long[]>(
				input => input.Select(v => (long)v).ToArray(),
				output => output.Select(v => (ulong)v).ToList()
			);

			modelBuilder.Entity<FilterCaps>(x => {
				x.Property(y => y.IgnoredChannels)
					.HasConversion(ulongConverter);
			});

			modelBuilder.Entity<FilterUrl>(x => {
				x.Property(y => y.IgnoredChannels)
					.HasConversion(ulongConverter);
			});
			
			modelBuilder.Entity<ServerInactivity>(x =>
			{
				x.HasMany(y => y.Users)
					.WithOne()
					.OnDelete(DeleteBehavior.Cascade);
				x.Property(y => y.UserWhitelist)
					.HasConversion(ulongConverter);
				x.Property(y => y.RoleWhitelist)
					.HasConversion(ulongConverter);
			});

			modelBuilder.Entity<ServerMusic>(x => {
				x.Property(y => y.PlaylistId)
					.HasConversion(input => input.ToString(), output => ObjectId.Parse(output));
				x.HasMany(y => y.AutoJoinConfigs)
					.WithOne()
					.OnDelete(DeleteBehavior.Cascade);
				x.Property(y => y.AllowedRoles)
					.HasConversion(ulongConverter);
			});

			modelBuilder.Entity<ServerRoleRequest>(x => {
				x.Property(y => y.RoleIds)
					.HasConversion(ulongConverter);
			});
			
			modelBuilder.Entity<ServerWarning>(x => {
				x.HasMany(y => y.Warnings)
					.WithOne()
					.OnDelete(DeleteBehavior.Cascade);
			});

			base.OnModelCreating(modelBuilder);
		}

		public int CheckForBadConfigs(IEnumerable<ulong> serverIds)
		{
			var badConfigCount = ServerProfiles.ConfigCheck(serverIds);

			if (badConfigCount > 0) SaveChanges();
			return badConfigCount;
		}

		public void DeleteGuildData(ulong id)
		{
			ServerProfiles.DeleteData(id);
			SaveChanges();
		}

		public override void Dispose()
		{
			SaveChanges();
			base.Dispose();
		}
	}
}