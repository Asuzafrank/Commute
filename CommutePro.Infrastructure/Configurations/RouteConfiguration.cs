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
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.HasKey(r => r.RouteId);

            builder.Property(r => r.RouteId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(r => r.RouteShortName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(r => r.RouteLongName)
                .HasMaxLength(255);

            builder.Property(r => r.RouteColor)
                  .HasMaxLength(50)
                  .IsFixedLength()
                  .HasDefaultValue("888888");  //default gray color

            builder.Property(r => r.RouteTextColor)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasDefaultValue("FFFFFF");

            builder.Property(r => r.AgencyId)
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(r => r.RouteShortName);
            builder.HasIndex(r => r.AgencyId);
        }
    }
}
