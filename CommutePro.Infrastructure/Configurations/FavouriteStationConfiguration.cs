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
    public class FavouriteStationConfiguration : IEntityTypeConfiguration<FavouriteStation>
    {
        public void Configure(EntityTypeBuilder<FavouriteStation> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            builder.Property(f => f.UserId)
                .IsRequired();

            builder.Property(f => f.StopId)
                .HasMaxLength(255)
                .IsRequired();

            builder.HasOne(f => f.User)
                .WithMany(u => u.FavouriteStations)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(f => new { f.UserId, f.StopId })
                .IsUnique();

            builder.HasIndex(f => new { f.UserId, f.SortOrder });
        }
    }
}
