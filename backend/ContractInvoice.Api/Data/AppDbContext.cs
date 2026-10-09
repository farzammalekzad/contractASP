using Microsoft.EntityFrameworkCore;
using ContractInvoice.Api.Model;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Contract> Contracts {get; set;}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>()
        .Property(c => c.ContractCode)
        .HasMaxLength(50);
        
        modelBuilder.Entity<Contract>()
            .HasIndex(c => c.ContractCode)
            .IsUnique();
    }  
}