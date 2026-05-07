using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Favourites;
using CommutePro.Application.Features.Favourites.Queries;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Favourites.Handlers
{
    public class GetFavouritesHandler : IRequestHandler<GetFavouritesQuery, BaseResponse<List<FavouriteStationDto>>>
    {
        private readonly IFavouriteRepository _favouriteRepository;
        private readonly IStationRepository _stationRepository;
        private readonly ICurrentUserService _currentUser;

        public GetFavouritesHandler(
            IFavouriteRepository favouriteRepository,
            IStationRepository stationRepository,
            ICurrentUserService currentUser)
        {
            _favouriteRepository = favouriteRepository;
            _stationRepository = stationRepository;
            _currentUser = currentUser;
        }
        public async Task<BaseResponse<List<FavouriteStationDto>>> Handle(GetFavouritesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return BaseResponse<List<FavouriteStationDto>>.Fail("User not authenticated");
            var favourites = await _favouriteRepository.GetByUserIdAsync(userId.Value, cancellationToken);

            var result = new List<FavouriteStationDto>();

            foreach (var fav in favourites.OrderBy(f => f.SortOrder))
            {
                var stop = await _stationRepository.GetByIdAsync(fav.StopId, cancellationToken);

                result.Add(new FavouriteStationDto
                {
                    Id = fav.Id,
                    StopId = fav.StopId,
                    StopName = stop?.StopName ?? fav.StopId,
                    PlatformCode = stop?.PlatformCode,
                    SortOrder = fav.SortOrder
                });
            }

            return BaseResponse<List<FavouriteStationDto>>.Ok(result);
        }
    }
}
