using Microsoft.EntityFrameworkCore;
using EnterpriseIT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseIT.Persistence.Context
{
    public class EnterpriseITDbContext : DbContext
    {
        public EnterpriseITDbContext(DbContextOptions<EnterpriseITDbContext> options)
            : base(options)
        {
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EnterpriseITDbContext).Assembly);
        }
    }
}