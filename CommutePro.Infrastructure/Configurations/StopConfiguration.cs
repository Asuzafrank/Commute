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
    public class StopConfiguration : IEntityTypeConfiguration<Stop>
    {
        public void Configure(EntityTypeBuilder<Stop> builder)
        {
            builder.HasKey(s => s.StopId);

            builder.Property(s => s.StopId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(s => s.StopName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(s => s.PlatformCode)
                .HasMaxLength(50);

            builder.Property(s => s.StopLat)
                .HasColumnType("decimal(10,7)");

            builder.Property(s => s.StopLon)
                .HasColumnType("decimal(10,7)");

            builder.Property(s => s.ParentStation)
                .HasMaxLength(255);

            // Indexes
            builder.HasIndex(s => s.StopName);
            builder.HasIndex(s => s.PlatformCode);
        }
    }
}
