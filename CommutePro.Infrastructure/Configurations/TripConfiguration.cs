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
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> builder)
        {
            builder.HasKey(t => t.TripId);

            builder.Property(t => t.TripId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(t => t.RouteId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(t => t.ServiceId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(t => t.TripHeadsign)
                .HasMaxLength(255);

            builder.Property(t => t.ShapeId)
                .HasMaxLength(255);

            builder.HasOne(t => t.Route)
                .WithMany(r => r.Trips)
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(t => t.RouteId);
            builder.HasIndex(t => t.ServiceId);
        }
    }
}
