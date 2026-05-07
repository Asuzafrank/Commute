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
    public class CalendarDateConfiguration : IEntityTypeConfiguration<CalendarDate>
    {
        public void Configure(EntityTypeBuilder<CalendarDate> builder)
        {
            builder.HasKey(cd => new { cd.ServiceId, cd.Date });

            builder.Property(cd => cd.ServiceId)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(cd => cd.Date)
                .HasColumnType("date");

            builder.HasOne(cd => cd.Service)
               .WithMany(s => s.CalendarDates)  
               .HasForeignKey(cd => cd.ServiceId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cd => cd.Date);
        }
    }
}
