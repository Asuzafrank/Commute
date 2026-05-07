using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Domain.Entities
{
    

    public class NotificationPreference
    {
        public Guid UserId { get; private set; }
        public int DelayThresholdMinutes { get; private set; }
        public bool PushEnabled { get; private set; }
        public bool EmailEnabled { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public ApplicationUser? User { get; private set; }

        private NotificationPreference() { }

        private NotificationPreference(Guid userId)
        {
            UserId = userId;
            DelayThresholdMinutes = 5;
            PushEnabled = true;
            EmailEnabled = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public static NotificationPreference Create(Guid userId)
        {
            return new NotificationPreference(userId);
        }

        public void Update(int delayThresholdMinutes, bool pushEnabled, bool emailEnabled)
        {
            if (delayThresholdMinutes < 1)
                throw new ArgumentException("Delay threshold must be at least 1 minute");

            if (delayThresholdMinutes > 60)
                throw new ArgumentException("Delay threshold cannot exceed 60 minutes");

            if (DelayThresholdMinutes == delayThresholdMinutes &&
                    PushEnabled == pushEnabled &&
                    EmailEnabled == emailEnabled)
                    return;

            DelayThresholdMinutes = delayThresholdMinutes;
            PushEnabled = pushEnabled;
            EmailEnabled = emailEnabled;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool ShouldNotifyForDelay(int delayMinutes)
        {
            return PushEnabled && delayMinutes >= DelayThresholdMinutes;
        }
    }
}
