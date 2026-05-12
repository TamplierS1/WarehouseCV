using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WarehouseCV.Models;

namespace WarehouseCV.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<Image> Images { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Inventory> Inventories { get; set; } = null!;

    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString =
                "Host=localhost;Port=5432;Database=WarehouseCV;Username=postgres;Password=64923;Include Error Detail=true";
            optionsBuilder.UseNpgsql(connectionString, o => o.UseNetTopologySuite());
        }
    }
}
