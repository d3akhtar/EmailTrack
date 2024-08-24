using EmailStatsService.Model;
using Microsoft.EntityFrameworkCore;

namespace EmailStatsService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration conf) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Property(m => m.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Team>().Property(m => m.Id).ValueGeneratedOnAdd();
        }

        public DbSet<User> Users {get; set;}
        public DbSet<Team> Teams { get; set; }
        public DbSet<Gmail> Gmails { get; set; }
    }
}