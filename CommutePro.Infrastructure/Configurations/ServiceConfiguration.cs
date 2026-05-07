using CommutePro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Configurations
{
    public class ServiceConfiguration : IEntityTypeConfiguration<Service>
    {
        public void Configure(EntityTypeBuilder<Service> builder)
        {
            builder.HasKey(s => s.ServiceId);

            builder.Property(s => s.ServiceId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(s => s.StartDate)
                .HasColumnType("date");

            builder.Property(s => s.EndDate)
                .HasColumnType("date");

            // Indexes for date queries
            builder.HasIndex(s => s.StartDate);
            builder.HasIndex(s => s.EndDate);

            builder.HasMany(s => s.CalendarDates)
            .WithOne(cd => cd.Service)
            .HasForeignKey(cd => cd.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
