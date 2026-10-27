using Microsoft.EntityFrameworkCore;
using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EnterpriseIT.Application.Interfaces;

namespace EnterpriseIT.Persistence.Context
{
    public class EnterpriseITDbContext : DbContext
    {
        private readonly IAuditActor? _auditActor;
        public int? AuditActorId => _auditActor?.UserId;

        public EnterpriseITDbContext(DbContextOptions<EnterpriseITDbContext> options, IAuditActor? auditActor = null)
            : base(options)
        {
            _auditActor = auditActor;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Department> Departments => Set<Department>();

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TicketComment> TicketComments => Set<TicketComment>();
        public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
        public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
        public DbSet<TicketPriority> TicketPriorities => Set<TicketPriority>();
        public DbSet<TicketStatus> TicketStatuses => Set<TicketStatus>();

        public DbSet<Asset> Assets => Set<Asset>();
        public DbSet<AssetType> AssetTypes => Set<AssetType>();
        public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
        public DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();

        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
            SaveChangesAsync(acceptAllChangesOnSuccess).GetAwaiter().GetResult();

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var changed = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
            if (changed.Any(e => e.Entity is AuditLog))
                throw new InvalidOperationException("Audit records can only be written by the server audit mechanism.");
            if (changed.Any(e => e.Entity is User && e.State == EntityState.Deleted))
                throw new InvalidOperationException("Users must be deactivated to preserve audit history.");
            if (!AuditActorId.HasValue || changed.Count == 0)
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            var pending = changed.Select(e => new
            {
                Entry = e, EntityName = e.Metadata.ClrType.Name,
                Key = e.Metadata.FindPrimaryKey()!.Properties.Single().Name,
                Added = e.State == EntityState.Added,
                Id = Convert.ToInt32(e.Property(e.Metadata.FindPrimaryKey()!.Properties.Single().Name).CurrentValue),
                Action = e.State == EntityState.Added ? "Create" : e.State == EntityState.Deleted ? "Delete"
                    : e.Entity is User && e.Property(nameof(User.PasswordHash)).IsModified ? "PasswordChanged" : "Update"
            }).ToList();

            await using var transaction = Database.CurrentTransaction == null
                ? await Database.BeginTransactionAsync(cancellationToken) : null;
            var count = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            foreach (var item in pending)
                await RecordAuditAsync(item.Action, item.EntityName,
                    item.Added ? Convert.ToInt32(item.Entry.Property(item.Key).CurrentValue) : item.Id, cancellationToken);
            if (transaction != null) await transaction.CommitAsync(cancellationToken);
            return count;
        }

        public async Task RecordAuditAsync(string action, string entityName, int entityId, CancellationToken cancellationToken = default)
        {
            var actorId = AuditActorId;
            if (!actorId.HasValue) return; // Development initialization has no authenticated HTTP actor.
            if (Database.CurrentTransaction == null)
                throw new InvalidOperationException("Audit and business changes require the same transaction.");
            if (entityId <= 0 || string.IsNullOrWhiteSpace(action) || action.Length > 100 ||
                string.IsNullOrWhiteSpace(entityName) || entityName.Length > 100)
                throw new InvalidOperationException("Invalid server audit metadata.");
            var now = DateTime.UtcNow;
            // Raw, parameterized insert avoids resaving tracked entities or recursively auditing audit records.
            await Database.ExecuteSqlInterpolatedAsync($"INSERT INTO \"AuditLogs\" (\"Action\", \"EntityName\", \"EntityId\", \"UserId\", \"CreatedAt\", \"UpdatedAt\") VALUES ({action}, {entityName}, {entityId}, {actorId.Value}, {now}, NULL)", cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnterpriseITDbContext).Assembly);
        }
    }
}
