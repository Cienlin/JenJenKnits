using Microsoft.EntityFrameworkCore;

namespace JenJenKnits.Data;

public class SiteDbContext(DbContextOptions<SiteDbContext> options) : DbContext(options)
{
    public DbSet<ProductRecord> Products => Set<ProductRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductRecord>(product =>
        {
            product.HasIndex(p => p.Slug).IsUnique();
            product.Property(p => p.Slug).HasMaxLength(80);
            product.Property(p => p.Name).HasMaxLength(100);
            product.Property(p => p.Category).HasMaxLength(50);
            product.Property(p => p.Material).HasMaxLength(200);
            product.Property(p => p.Dimensions).HasMaxLength(200);
            product.Property(p => p.ShortDescription).HasMaxLength(500);
            product.Property(p => p.BuyUrl).HasMaxLength(500);
            product.Property(p => p.ColorSimulator).HasMaxLength(50);
        });
    }
}
