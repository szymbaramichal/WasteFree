using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using TickerQ.EntityFrameworkCore.Configurations;
using WasteFree.Domain.Entities;
using WasteFree.Domain.Interfaces;
using WasteFree.Domain.Models;

namespace WasteFree.Infrastructure;
public class ApplicationDataContext : DbContext
{
    private readonly ICurrentUserService currentUserService;
    private readonly IGeocodingService geocodingService;
    private readonly ILogger<ApplicationDataContext> logger;

    public ApplicationDataContext(
        DbContextOptions options,
        ICurrentUserService currentUserService,
        IGeocodingService geocodingService,
        ILogger<ApplicationDataContext> logger) : base(options)
    {
        this.currentUserService = currentUserService;
        this.geocodingService = geocodingService;
        this.logger = logger;
    }

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

        modelBuilder.Entity<GarbageOrder>()
            .HasOne(o => o.AssignedGarbageAdmin)
            .WithMany()
            .HasForeignKey(o => o.AssignedGarbageAdminId)
            .OnDelete(DeleteBehavior.SetNull);

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
        GeocodeAddressesAsync(CancellationToken.None).GetAwaiter().GetResult();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        await GeocodeAddressesAsync(cancellationToken);
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

    private async Task GeocodeAddressesAsync(CancellationToken cancellationToken)
    {
        var addressEntries = ChangeTracker.Entries<Address>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in addressEntries)
        {
            if (!ShouldGeocode(entry) || !HasRequiredAddressData(entry.Entity))
            {
                continue;
            }

            try
            {
                var coordinates = await geocodingService.TryGetCoordinatesAsync(entry.Entity, cancellationToken);
                if (!coordinates.HasValue)
                {
                    continue;
                }

                entry.Entity.Latitude = coordinates.Value.Latitude;
                entry.Entity.Longitude = coordinates.Value.Longitude;

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(Address.Latitude)).IsModified = true;
                    entry.Property(nameof(Address.Longitude)).IsModified = true;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to resolve coordinates for address {Street}, {PostalCode}, {City}",
                    entry.Entity.Street,
                    entry.Entity.PostalCode,
                    entry.Entity.City);
            }
        }
    }

    private static bool HasRequiredAddressData(Address address)
    {
        return !string.IsNullOrWhiteSpace(address.Street) &&
               !string.IsNullOrWhiteSpace(address.PostalCode) &&
               !string.IsNullOrWhiteSpace(address.City);
    }

    private static bool ShouldGeocode(EntityEntry<Address> entry)
    {
        if (entry.State == EntityState.Added)
        {
            return true;
        }

        if (!entry.Entity.Latitude.HasValue || !entry.Entity.Longitude.HasValue)
        {
            return true;
        }

        return entry.Property(nameof(Address.City)).IsModified ||
               entry.Property(nameof(Address.PostalCode)).IsModified ||
               entry.Property(nameof(Address.Street)).IsModified;
    }
}