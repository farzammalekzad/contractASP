using Microsoft.EntityFrameworkCore;
using ContractInvoice.Api.Model;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Contract> Contracts {get; set;}
    public DbSet<ProgressPayment> ProgressPayments { get; set;}
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Principal> Principals { get; set; }
    public DbSet<ContractAttachment> ContractAttachments { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.Property(c => c.ContractNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(c => c.ContractNo)
                .IsUnique();

            entity.Property(c => c.Price)
                .HasPrecision(18, 2);

            entity.Property(c => c.OutSourcePrice)
                .HasPrecision(18, 2);

            entity.Property(c => c.Type)
                .HasMaxLength(20);

            entity.Property(c => c.Status)
                .HasMaxLength(30);
            
            entity.Property(c => c.AdvancePaymentAmount)
                .HasPrecision(18,2);
            
            entity.HasOne(c => c.Principal)
                .WithMany(p => p.Contracts)
                .HasForeignKey(c => c.PrincipalId)
                .OnDelete(DeleteBehavior.Restrict);
              
        });
       
        modelBuilder.Entity<ProgressPayment>(entity =>
        {
            entity.ToTable("invoice");
            entity.Property(p => p.Title).HasMaxLength(250);
            entity.Property(p => p.PaymentNo).HasColumnName("No");
            entity.Property(p => p.PaymentType).HasMaxLength(30).HasColumnName("Type");
            entity.Property(p => p.PeriodStartDate).HasMaxLength(20).HasColumnName("InvoiceStartDte");
            entity.Property(p => p.PeriodEndDate).HasMaxLength(20).HasColumnName("InvoiceEndDte");
            entity.Property(p => p.RequestLetterNo).HasMaxLength(100).HasColumnName("OceRequestLetterNo");
            entity.Property(p => p.RequestLetterDate).HasMaxLength(20).HasColumnName("OceRequestLetterDate");
            entity.Property(p => p.ApprovalLetterNo).HasMaxLength(100).HasColumnName("PrincipalApprovedLetterNo");
            entity.Property(p => p.ApprovalDate).HasMaxLength(20).HasColumnName("PrincipalApprovedLetterDate");
            entity.Property(p => p.Status).HasMaxLength(50);
            entity.Property(p => p.ProposedAmount).HasPrecision(18, 2).HasColumnName("RequestPrice");
            entity.Property(p => p.ApprovedAmount).HasPrecision(18, 2).HasColumnName("ApprovedPrice");
            entity.Property(p => p.Header_ContractNo).HasMaxLength(30).HasColumnName("Header_ContractNo");

            entity.HasIndex(p => new { p.ContractId, p.PaymentNo }).IsUnique();
            entity.HasOne(p => p.Contract)
                .WithMany(c => c.ProgressPayments)
                .HasForeignKey(p => p.ContractId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.ProgressPayment)
            .WithMany(pp => pp.Payments)
            .HasForeignKey(p => p.ProgressPaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .Property(p => p.ReceivedAmount)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentDate)
            .HasMaxLength(20);

        modelBuilder.Entity<Payment>()
            .Property(p => p.PaymentReferenceNo)
            .HasMaxLength(100);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Description)
            .HasMaxLength(500);
        modelBuilder.Entity<ContractAttachment>(entity =>
        {
            entity.Property(a => a.FileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(a => a.StoredFileName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(a => a.DocumentType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(a => a.ContentType)
                .HasMaxLength(100);

            entity.HasOne(a => a.Contract)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.ContractId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }  
}
