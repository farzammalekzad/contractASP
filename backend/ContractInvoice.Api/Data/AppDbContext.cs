using Microsoft.EntityFrameworkCore;
using ContractInvoice.Api.Model;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Contract> Contracts {get; set;}
    public DbSet<ProgressPayment> ProgressPayments { get; set;}
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>()
        .Property(c => c.ContractCode)
        .HasMaxLength(50);
        
        modelBuilder.Entity<Contract>()
            .HasIndex(c => c.ContractCode)
            .IsUnique();

        modelBuilder.Entity<ProgressPayment>(entity =>
        {
            entity.Property(p => p.Title).HasMaxLength(250);
            entity.Property(p => p.PaymentType).HasMaxLength(30);
            entity.Property(p => p.PeriodStartDate).HasMaxLength(20);
            entity.Property(p => p.PeriodEndDate).HasMaxLength(20);
            entity.Property(p => p.RequestLetterNo).HasMaxLength(100);
            entity.Property(p => p.RequestLetterDate).HasMaxLength(20);
            entity.Property(p => p.ApprovalLetterNo).HasMaxLength(100);
            entity.Property(p => p.ApprovalDate).HasMaxLength(20);
            entity.Property(p => p.Status).HasMaxLength(50);
            entity.Property(p => p.ProposedAmount).HasPrecision(18, 2);
            entity.Property(p => p.ApprovedAmount).HasPrecision(18, 2);

            entity.HasIndex(p => new { p.ContractId, p.PaymentNo }).IsUnique();
            entity.HasOne(p => p.Contract)
                .WithMany(c => c.ProgressPayments)
                .HasForeignKey(p => p.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }  
}
