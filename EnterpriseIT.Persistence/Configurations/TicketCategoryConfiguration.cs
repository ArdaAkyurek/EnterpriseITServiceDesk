using EnterpriseIT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseIT.Persistence.Configurations
{
    public class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
    {
        public void Configure(EntityTypeBuilder<TicketCategory> builder)
        {
            // Primary Key
            builder.HasKey(x => x.TicketCategoryId);

            // Table Name
            builder.ToTable("TicketCategories");

            // Properties
            builder.Property(x => x.CategoryName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Description)
                   .HasMaxLength(250);

            builder.HasData(
    new TicketCategory
    {
        TicketCategoryId = 1,
        CategoryName = "Hardware",
        Description = "Hardware related issues"
    },
    new TicketCategory
    {
        TicketCategoryId = 2,
        CategoryName = "Software",
        Description = "Software related issues"
    },
    new TicketCategory
    {
        TicketCategoryId = 3,
        CategoryName = "Network",
        Description = "Network related issues"
    }
);
        }
    }
}