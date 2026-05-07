using CommutePro.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Favourites.Commands
{
    public class ReorderFavouritesCommand : IRequest<BaseResponse<bool>>
    {
        public Guid UserId { get; set; }
        public Dictionary<Guid, int> NewOrder { get; set; } = new();
    }
}
