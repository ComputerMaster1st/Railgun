using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TreeDiagram.Models;
using TreeDiagram.Models.Filter;
using TreeDiagram.Models.Fun;
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
			modelBuilder.Entity<ServerMusic>(x => {
				x.Property(y => y.PlaylistId)
					.HasConversion(input => input.ToString(), output => ObjectId.Parse(output));
			});
			
			modelBuilder.Entity<ServerInactivity>(x =>
			{
				x.HasMany(f => f.Users).WithOne().OnDelete(DeleteBehavior.Cascade);
			});
			
            modelBuilder.Entity<ServerWarning>().HasMany(f => f.Warnings).WithOne().OnDelete(DeleteBehavior.Cascade);
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