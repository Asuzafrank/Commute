using CommutePro.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Favourites.Queries
{
    public class CheckFavouriteQuery : IRequest<BaseResponse<bool>>
    {
        public Guid UserId { get; set; }
        public string StopId { get; set; } = string.Empty;
    }
}
