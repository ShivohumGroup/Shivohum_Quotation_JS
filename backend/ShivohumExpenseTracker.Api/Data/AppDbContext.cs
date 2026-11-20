using Microsoft.EntityFrameworkCore;
using ShivohumExpenseTracker.Api.Models.Entities;

namespace ShivohumExpenseTracker.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<PaymentMode> PaymentModes => Set<PaymentMode>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ClientPayment> ClientPayments => Set<ClientPayment>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.ProjectName).HasMaxLength(150).IsRequired();
            entity.Property(e => e.ClientName).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ContractAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<PaymentMode>(entity =>
        {
            entity.ToTable("PaymentModes");
            entity.HasKey(e => e.PaymentModeId);
            entity.Property(e => e.PaymentModeName).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.AccountId);
            entity.Property(e => e.AccountName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.AccountType).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Category).WithMany(c => c.Transactions).HasForeignKey(e => e.CategoryId);
            entity.HasOne(e => e.Project).WithMany(p => p.Transactions).HasForeignKey(e => e.ProjectId);
            entity.HasOne(e => e.PaymentMode).WithMany(p => p.Transactions).HasForeignKey(e => e.PaymentModeId);
            entity.HasOne(e => e.Account).WithMany(a => a.Transactions).HasForeignKey(e => e.AccountId);
            entity.HasOne(e => e.CreatedByUser).WithMany(u => u.Transactions).HasForeignKey(e => e.CreatedBy);
        });

        modelBuilder.Entity<ClientPayment>(entity =>
        {
            entity.ToTable("ClientPayments");
            entity.HasKey(e => e.ClientPaymentId);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReferenceNo).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Project).WithMany(p => p.ClientPayments).HasForeignKey(e => e.ProjectId);
            entity.HasOne(e => e.PaymentMode).WithMany(p => p.ClientPayments).HasForeignKey(e => e.PaymentModeId);
            entity.HasOne(e => e.Account).WithMany(a => a.ClientPayments).HasForeignKey(e => e.AccountId);
            entity.HasOne(e => e.CreatedByUser).WithMany(u => u.ClientPayments).HasForeignKey(e => e.CreatedBy);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Documents");
            entity.HasKey(e => e.DocumentId);
            entity.Property(e => e.Kind).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PayloadJson).IsRequired();
            entity.HasOne(e => e.Project).WithMany(p => p.Documents).HasForeignKey(e => e.ProjectId);
            entity.HasOne(e => e.CreatedByUser).WithMany(u => u.Documents).HasForeignKey(e => e.CreatedBy);
        });
    }
}
