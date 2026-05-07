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
    public class StopTimeConfiguration : IEntityTypeConfiguration<StopTime>
    {
        public void Configure(EntityTypeBuilder<StopTime> builder)
        {
            builder.HasKey(st => new { st.TripId, st.StopSequence });

            builder.Property(st => st.TripId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(st => st.StopId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(st => st.StopSequence)
                .IsRequired();

            builder.Property(st => st.ArrivalTime)
                .HasColumnType("time");

            builder.Property(st => st.DepartureTime)
                .HasColumnType("time");

            builder.HasOne(st => st.Trip)
                .WithMany(t => t.StopTimes)
                .HasForeignKey(st => st.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(st => st.Stop)
                .WithMany(s => s.StopTimes)
                .HasForeignKey(st => st.StopId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(st => st.StopId);
            builder.HasIndex(st => new { st.StopId, st.DepartureTime });
            builder.HasIndex(st => new { st.TripId, st.StopSequence });
        }
    }
}
