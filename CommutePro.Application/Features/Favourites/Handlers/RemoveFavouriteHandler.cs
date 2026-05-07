using CommutePro.Application.Common;
using CommutePro.Application.Features.Favourites.Commands;
using CommutePro.Application.Interfaces;
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
    // Application/Features/Favourites/Handlers/RemoveFavouriteHandler.cs
    public class RemoveFavouriteHandler : IRequestHandler<RemoveFavouriteCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public RemoveFavouriteHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<BaseResponse<bool>> Handle(RemoveFavouriteCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return BaseResponse<bool>.Fail("User not authenticated");
            var user = await _unitOfWork.Users.GetWithFavouritesAsync(userId.Value, cancellationToken);
            if (user == null)
                return BaseResponse<bool>.Fail($"User {userId.Value} not found");

            var favourite = user.FavouriteStations.FirstOrDefault(f => f.Id == request.FavouriteId);
            if (favourite == null)
                return BaseResponse<bool>.Fail($"Favourite {request.FavouriteId} not found");

            user.RemoveFavouriteStation(request.FavouriteId);

            await _unitOfWork.SaveChangesAsync(cancellationToken);  

            return BaseResponse<bool>.Ok(true, "Favourite removed successfully");
        }
    }
}
