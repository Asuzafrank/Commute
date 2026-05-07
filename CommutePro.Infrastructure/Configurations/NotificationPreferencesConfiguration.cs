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
    public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreference>
    {
        public void Configure(EntityTypeBuilder<NotificationPreference> builder)
        {
            builder.HasKey(np => np.UserId);

            builder.Property(np => np.DelayThresholdMinutes)
                .IsRequired()
                .HasDefaultValue(5);

            builder.Property(np => np.PushEnabled)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(np => np.EmailEnabled)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(np => np.User)
                .WithOne(u => u.NotificationPreferences)
                .HasForeignKey<NotificationPreference>(np => np.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
