using Microsoft.EntityFrameworkCore;

public class FileScanDbContext : DbContext
{
    public DbSet<FileScanInfo> hashes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=hashes.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileScanInfo>()
            .HasKey(f => f.sha256);

        base.OnModelCreating(modelBuilder);
    }
}
