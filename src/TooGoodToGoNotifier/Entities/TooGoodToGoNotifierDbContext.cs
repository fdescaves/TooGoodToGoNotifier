using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace TooGoodToGoNotifier.Entities
{
    public class TooGoodToGoNotifierDbContext : DbContext
    {
        public TooGoodToGoNotifierDbContext(DbContextOptions<TooGoodToGoNotifierDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .Property(x => x.FavoriteBaskets)
                .HasConversion(
                    x => JsonSerializer.Serialize(x, new JsonSerializerOptions()),
                    x => JsonSerializer.Deserialize<List<string>>(x, new JsonSerializerOptions())
                );
        }
    }
}
