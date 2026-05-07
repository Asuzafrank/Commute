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
    public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> builder)
        {
            builder.HasKey(a => a.AgencyId);

            builder.Property(a => a.AgencyId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.AgencyName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.AgencyUrl)
                .HasMaxLength(255);

            builder.Property(a => a.AgencyTimezone)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(a => a.AgencyLang)
                .HasMaxLength(10);

            builder.Property(a => a.AgencyPhone)
                .HasMaxLength(100);
        }
    }
}
