using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Interfaces.Services
{
    public interface IDelayDetectionService
    {
        Task DetectAndNotifyDelaysAsync(CancellationToken cancellationToken = default);
    }
}
