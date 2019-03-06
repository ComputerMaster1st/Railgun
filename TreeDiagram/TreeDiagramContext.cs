using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TreeDiagram.Models.Server;
using TreeDiagram.Models.Server.Filter;
using TreeDiagram.Models.Server.Fun;
using TreeDiagram.Models.TreeTimer;
using TreeDiagram.Models.User;

namespace TreeDiagram
{
    public sealed class TreeDiagramContext : DbContext
    {
        public DbSet<FilterCaps> FilterCapses { get; internal set; }
        public DbSet<FilterUrl> FilterUrls { get; internal set; }

        public DbSet<FunBite> FunBites { get; internal set; }
        public DbSet<FunRst> FunRsts { get; internal set; }

        public DbSet<ServerCommand> ServerCommands { get; internal set; }
        public DbSet<ServerInactivity> ServerInactivities { get; internal set; }
		public DbSet<ServerJoinLeave> ServerJoinLeaves { get; internal set; }
		public DbSet<ServerMention> ServerMentions { get; internal set; }
		public DbSet<ServerMusic> ServerMusics { get; internal set; }
		public DbSet<ServerWarning> ServerWarnings { get; internal set; }

	    public DbSet<TimerAssignRole> TimerAssignRoles { get; internal set; }
	    public DbSet<TimerKickUser> TimerKickUsers { get; internal set; }
	    public DbSet<TimerRemindMe> TimerRemindMes { get; internal set; }

	    public DbSet<UserCommand> UserCommands { get; internal set; }
		public DbSet<UserMention> UserMentions { get; internal set; }

        public TreeDiagramContext(DbContextOptions optionsBuilder) : base(optionsBuilder) => AppContext.SetSwitch("System.Net.Http.useSocketsHttpHandler", false);

	    protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ServerMusic>(x => {
				x.Property(y => y.PlaylistId)
					.HasConversion(input => input.ToString(), output => ObjectId.Parse(output));
				x.HasMany(f => f.AllowedRoles).WithOne().OnDelete(DeleteBehavior.Cascade);
			});
			modelBuilder.Entity<ServerInactivity>(x =>
			{
				x.HasMany(f => f.Users).WithOne().OnDelete(DeleteBehavior.Cascade);
				x.HasMany(f => f.RoleWhitelist).WithOne().OnDelete(DeleteBehavior.Cascade);
				x.HasMany(f => f.UserWhitelist).WithOne().OnDelete(DeleteBehavior.Cascade);
			});
			
            modelBuilder.Entity<ServerWarning>().HasMany(f => f.Warnings).WithOne().OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<FilterCaps>().HasMany(f => f.IgnoredChannels).WithOne().OnDelete(DeleteBehavior.Cascade);
			modelBuilder.Entity<FilterUrl>().HasMany(f => f.IgnoredChannels).WithOne().OnDelete(DeleteBehavior.Cascade);
			base.OnModelCreating(modelBuilder);
		}

		public void DeleteGuildData(ulong id)
		{
			var filterCaps = FilterCapses.FirstOrDefault(find => find.Id == id);
			var filterUrl = FilterUrls.FirstOrDefault(find => find.Id == id);
			var funBites = FunBites.FirstOrDefault(find => find.Id == id);
			var funRst = FunRsts.FirstOrDefault(find => find.Id == id);
			var serverCommand = ServerCommands.FirstOrDefault(find => find.Id == id);
            var serverInactivity = ServerInactivities.FirstOrDefault(find => find.Id == id);
            var serverMention = ServerMentions.FirstOrDefault(find => find.Id == id);
			var serverMusic = ServerMusics.FirstOrDefault(find => find.Id == id);
			var serverWarning = ServerWarnings.FirstOrDefault(find => find.Id == id);
			var serverJoinLeave = ServerJoinLeaves.FirstOrDefault(find => find.Id == id);

			if (filterCaps != null) FilterCapses.Remove(filterCaps);
			if (filterUrl != null) FilterUrls.Remove(filterUrl);
			if (funBites != null) FunBites.Remove(funBites);
			if (funRst != null) FunRsts.Remove(funRst);
			if (serverCommand != null) ServerCommands.Remove(serverCommand);
            if (serverInactivity != null) ServerInactivities.Remove(serverInactivity);
            if (serverMention != null) ServerMentions.Remove(serverMention);
			if (serverMusic != null) ServerMusics.Remove(serverMusic);
			if (serverWarning != null) ServerWarnings.Remove(serverWarning);
			if (serverJoinLeave != null) ServerJoinLeaves.Remove(serverJoinLeave);

			SaveChanges();
		}

		public override void Dispose()
		{
			SaveChanges();
			base.Dispose();
		}
	}
}