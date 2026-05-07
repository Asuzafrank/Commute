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
    // Application/Features/Favourites/Handlers/ReorderFavouritesHandler.cs
    public class ReorderFavouritesHandler : IRequestHandler<ReorderFavouritesCommand, BaseResponse<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;  // ← CHANGE
        private readonly ICurrentUserService _currentUser;

        public ReorderFavouritesHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<BaseResponse<bool>> Handle(ReorderFavouritesCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId;
            if (!userId.HasValue)
                return BaseResponse<bool>.Fail("User not authenticated");
            var user = await _unitOfWork.Users.GetWithFavouritesAsync(userId.Value, cancellationToken);
            if (user == null)
                return BaseResponse<bool>.Fail($"User {userId.Value} not found");

            user.ReorderFavourites(request.NewOrder);

            await _unitOfWork.SaveChangesAsync(cancellationToken); 

            return BaseResponse<bool>.Ok(true, "Favourites reordered successfully");
        }
    }
}
