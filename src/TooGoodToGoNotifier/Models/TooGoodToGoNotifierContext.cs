using Microsoft.EntityFrameworkCore;

namespace TooGoodToGoNotifier.Models
{
    public class TooGoodToGoNotifierContext : DbContext
    {
        public TooGoodToGoNotifierContext(DbContextOptions<TooGoodToGoNotifierContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=TooGoodToGoNotifier.db;Cache=Shared");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Basket> Baskets { get; set; }

        public DbSet<Store> Stores { get; set; }
    }
}
