using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Favourites;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Favourites.Commands
{
    public class AddFavouriteCommand : IRequest<BaseResponse<FavouriteStationDto>>
    {
        public Guid UserId { get; set; }
        public string StopId { get; set; } = string.Empty;
    }
}
