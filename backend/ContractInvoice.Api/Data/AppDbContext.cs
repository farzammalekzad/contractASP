using Microsoft.EntityFrameworkCore;
using ContractInvoice.Api.Model;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    public DbSet<Contract> Contracts {get; set;}
    public DbSet<ProgressPayment> ProgressPayments { get; set;}
    // Retained only for legacy source compatibility; Payment is excluded from the EF model below.
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentDocument> PaymentDocuments { get; set; }
    public DbSet<PaymentAllocation> PaymentAllocations { get; set; }
    public DbSet<Principal> Principals { get; set; }
    public DbSet<ContractAttachment> ContractAttachments { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Prevent the legacy DbSet and navigation from rediscovering the retired entity.
        modelBuilder.Ignore<Payment>();

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
            entity.Ignore(p => p.Payments);
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

        modelBuilder.Entity<PaymentDocument>(entity =>
        {
            entity.ToTable("PaymentDocuments", table =>
                table.HasCheckConstraint("CK_PaymentDocuments_TotalAmount", "[TotalAmount] > 0"));

            entity.Property(p => p.PaymentReferenceNo).HasMaxLength(100).IsRequired();
            entity.HasIndex(p => p.PaymentReferenceNo).IsUnique();
            entity.Property(p => p.PaymentDate).HasMaxLength(20).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(500).IsRequired(false);
            entity.Property(p => p.TotalAmount).HasPrecision(18, 2);
            entity.Property(p => p.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<PaymentAllocation>(entity =>
        {
            entity.ToTable("PaymentAllocations", table =>
            {
                table.HasCheckConstraint("CK_PaymentAllocations_AllocatedAmount", "[AllocatedAmount] > 0");
                table.HasCheckConstraint("CK_PaymentAllocations_AllocationType",
                    $"[AllocationType] IN ({(int)AllocationType.Full}, {(int)AllocationType.Partial})");
                table.HasCheckConstraint("CK_PaymentAllocations_GoodPerformance", "[GoodPerformance] >= 0");
                table.HasCheckConstraint("CK_PaymentAllocations_InsuranceDeposit", "[InsuranceDeposit] >= 0");
                table.HasCheckConstraint("CK_PaymentAllocations_ObligationsDeposit", "[ObligationsDeposit] >= 0");
                table.HasCheckConstraint("CK_PaymentAllocations_VatReceivable", "[VatReceivable] >= 0");
                table.HasCheckConstraint("CK_PaymentAllocations_RadiationAllowance", "[RadiationAllowance] >= 0");
                table.HasCheckConstraint("CK_PaymentAllocations_ForeignMission", "[ForeignMission] >= 0");
            });

            entity.Property(p => p.AllocatedAmount).HasPrecision(18, 2);
            entity.Property(p => p.AllocationType).HasConversion<int>();
            entity.Property(p => p.GoodPerformance).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(p => p.InsuranceDeposit).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(p => p.ObligationsDeposit).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(p => p.VatReceivable).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(p => p.RadiationAllowance).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(p => p.ForeignMission).HasPrecision(18, 2).HasDefaultValue(0m);

            entity.HasIndex(p => new { p.PaymentDocumentId, p.ProgressPaymentId }).IsUnique();
            entity.HasIndex(p => p.ProgressPaymentId);

            entity.HasOne(p => p.PaymentDocument)
                .WithMany(d => d.Allocations)
                .HasForeignKey(p => p.PaymentDocumentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.ProgressPayment)
                .WithMany(pp => pp.Allocations)
                .HasForeignKey(p => p.ProgressPaymentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

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
