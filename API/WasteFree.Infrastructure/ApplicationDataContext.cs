using Microsoft.EntityFrameworkCore;
using TickerQ.EntityFrameworkCore.Configurations;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Infrastructure;
public class ApplicationDataContext(DbContextOptions options, ICurrentUserService currentUserService) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<GarbageGroup> GarbageGroups { get; set; }
    public DbSet<UserGarbageGroup> UserGarbageGroups { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<WalletTransaction> WalletTransactions { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<InboxNotification> InboxNotifications { get; set; }
    public DbSet<GarbageAdminConsent> GarbageAdminConsents { get; set; }
    public DbSet<GarbageOrder> GarbageOrders { get; set; }
    public DbSet<GarbageOrderUsers> GarbageOrderUsers { get; set; }
    public DbSet<GarbageGroupMessage> GarbageGroupMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TimeTickerConfigurations());
        modelBuilder.ApplyConfiguration(new CronTickerConfigurations());
        modelBuilder.ApplyConfiguration(new CronTickerOccurrenceConfigurations());

        modelBuilder.Entity<User>()
            .HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<Wallet>(w => w.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .OwnsOne(u => u.Address);

        modelBuilder.Entity<User>()
            .HasMany(u => u.InboxNotifications)
            .WithOne(w => w.User)
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GarbageGroup>()
            .OwnsOne(u => u.Address);

        modelBuilder.Entity<GarbageGroupMessage>()
            .Property(m => m.Content)
            .HasMaxLength(2000)
            .IsRequired();

        modelBuilder.Entity<GarbageGroupMessage>()
            .HasOne(m => m.GarbageGroup)
            .WithMany(g => g.Messages)
            .HasForeignKey(m => m.GarbageGroupId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GarbageGroupMessage>()
            .HasOne(m => m.User)
            .WithMany(u => u.GarbageGroupMessages)
            .HasForeignKey(m => m.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GarbageGroupMessage>()
            .HasIndex(m => new { m.GarbageGroupId, m.CreatedDateUtc });
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e is { Entity: DatabaseEntity, State: EntityState.Added or EntityState.Modified });
        var utcNow = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            var entity = (DatabaseEntity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedDateUtc = utcNow;
                entity.CreatedBy = currentUserService.UserId;;
            }
            else
            {
                entity.ModifiedDateUtc = utcNow;
                entity.ModifiedBy = currentUserService.UserId;   
            }
        }
    }
}