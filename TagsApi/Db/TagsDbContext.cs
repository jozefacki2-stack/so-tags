using Microsoft.EntityFrameworkCore;
using TagsApi.Models;

namespace TagsApi.Db
{
    public class TagsDbContext : DbContext
    {
        public DbSet<TagEntity> Tags => Set<TagEntity>();

        public TagsDbContext(DbContextOptions<TagsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TagEntity>().HasIndex(x => x.Name).IsUnique();
        }
    }
}
