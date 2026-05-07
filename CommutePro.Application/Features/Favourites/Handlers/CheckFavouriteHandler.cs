using CommutePro.Application.Common;
using CommutePro.Application.Features.Favourites.Queries;
using CommutePro.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Favourites.Handlers
{
    public class CheckFavouriteHandler : IRequestHandler<CheckFavouriteQuery, BaseResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public CheckFavouriteHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BaseResponse<bool>> Handle(CheckFavouriteQuery request, CancellationToken cancellationToken)
        {
            var exists = await _context.FavouriteStations
                .AnyAsync(f => f.UserId == request.UserId && f.StopId == request.StopId, cancellationToken);

            return BaseResponse<bool>.Ok(exists);
        }
    }
}
