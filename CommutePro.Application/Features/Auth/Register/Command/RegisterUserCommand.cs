using CommutePro.Application.Common;
using CommutePro.Application.DTOs.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Application.Features.Auth.Register.Command
{
    public class RegisterUserCommand : IRequest<BaseResponse<AuthResponse>>
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
