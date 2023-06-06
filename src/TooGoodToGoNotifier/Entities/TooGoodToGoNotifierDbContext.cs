using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
                    x => JsonSerializer.Deserialize<List<string>>(x, new JsonSerializerOptions()),
                     new ValueComparer<List<string>>((c1, c2) => c1.SequenceEqual(c2), c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())), c => c.ToList())
                );
        }
    }
}
