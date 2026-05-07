using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Favourites;
using CommutePro.Application.Features.Favourites.Commands;
using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using CommutePro.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Favourites.Handlers
{
    // Application/Features/Favourites/Handlers/AddFavouriteHandler.cs
    public class AddFavouriteHandler : IRequestHandler<AddFavouriteCommand, BaseResponse<FavouriteStationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AddFavouriteHandler> _logger;

        public AddFavouriteHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ILogger<AddFavouriteHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<BaseResponse<FavouriteStationDto>> Handle(AddFavouriteCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return BaseResponse<FavouriteStationDto>.Fail("User not authenticated");

            // Check if user exists
            var userExists = await _unitOfWork.Users.ExistsAsync(userId.Value, cancellationToken);
            if (!userExists)
                return BaseResponse<FavouriteStationDto>.Fail($"User {userId.Value} not found");

            // Check if stop exists
            var stop = await _unitOfWork.Stations.GetByIdAsync(request.StopId, cancellationToken);
            if (stop == null)
                return BaseResponse<FavouriteStationDto>.Fail($"Stop {request.StopId} not found");

            // Check if already favourite
            var alreadyFavourite = await _unitOfWork.Favourites.ExistsAsync(userId.Value, request.StopId, cancellationToken);
            if (alreadyFavourite)
                return BaseResponse<FavouriteStationDto>.Fail($"Station {request.StopId} is already in favourites");

            // Get max sort order for this user
            var existingFavourites = await _unitOfWork.Favourites.GetByUserIdAsync(userId.Value, cancellationToken);
            var maxSortOrder = existingFavourites.Any() ? existingFavourites.Max(f => f.SortOrder) : 0;

            // Create favourite using your factory method
            var favourite = FavouriteStation.Create(userId.Value, request.StopId, maxSortOrder + 1);

            await _unitOfWork.Favourites.AddAsync(favourite, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new FavouriteStationDto
            {
                Id = favourite.Id,
                StopId = favourite.StopId,
                StopName = stop.StopName,
                PlatformCode = stop.PlatformCode,
                SortOrder = favourite.SortOrder
            };

            return BaseResponse<FavouriteStationDto>.Ok(dto, "Favourite added successfully");
        }
    }
}
