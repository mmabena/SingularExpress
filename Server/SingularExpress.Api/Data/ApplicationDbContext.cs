using Microsoft.EntityFrameworkCore;
using SingularExpress.Api.Models;

namespace SingularExpress.Api.Data;

/// <summary>
/// EF Core database context. Add a DbSet for each new table and configure it in
/// <see cref="OnModelCreating"/>. After changing a model, create a migration:
/// <c>dotnet ef migrations add &lt;Name&gt; --project SingularExpress.Api</c>
/// </summary>
public class ApplicationDbContext : DbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
  {
  }

  public DbSet<User> Users => Set<User>();

  public DbSet<PasswordResetPin> PasswordResetPins => Set<PasswordResetPin>();

  public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

  public DbSet<FinancialPeriod> FinancialPeriods => Set<FinancialPeriod>();

  public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<User>(entity =>
    {
      entity.HasIndex(u => u.Email).IsUnique();
      entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
      entity.Property(u => u.PasswordHash).IsRequired();
      entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(32);
    });

    modelBuilder.Entity<PasswordResetPin>(entity =>
    {
      entity.HasIndex(p => new { p.UserId, p.IsUsed, p.ExpiresAtUtc });
      entity.Property(p => p.PinHash).IsRequired();
      entity.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<PasswordHistory>(entity =>
    {
      entity.HasIndex(h => new { h.UserId, h.ChangedAtUtc });
      entity.HasOne(h => h.User).WithMany().HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<FinancialPeriod>(entity =>
    {
      // A month can exist only once.
      entity.HasIndex(p => new { p.FinancialYearStart, p.PeriodNumber }).IsUnique();

      // Only one row may be Open at a time. The database enforces this even if two
      // SuperUsers click "Roll over" at the same moment.
      entity.HasIndex(p => p.Status)
        .IsUnique()
        .HasFilter("[Status] = 'Open'")
        .HasDatabaseName("IX_FinancialPeriods_SingleOpen");

      entity.Property(p => p.Status).HasConversion<string>().HasMaxLength(16);
    });

    modelBuilder.Entity<AuditLog>(entity =>
    {
      entity.Property(a => a.Action).HasMaxLength(64).IsRequired();
      entity.Property(a => a.EntityName).HasMaxLength(64);
      entity.Property(a => a.EntityId).HasMaxLength(64);
      entity.Property(a => a.Details).HasMaxLength(1024);
      entity.HasIndex(a => a.CreatedAtUtc);
    });
  }
}
