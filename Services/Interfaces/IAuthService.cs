using HadiyahServices.DTOs.Common;
using HadiyahServices.DTOs.User;

namespace HadiyahServices.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<string>> Register(RegisterDto dto);
        Task<BaseResponse<string>> Login(LoginDto dto);
        Task<BaseResponse<string>> ForgotPassword(string email);
        Task<BaseResponse<string>> VerifyResetCode(string email, string code);
        Task<BaseResponse<string>> ResetPassword(ResetPasswordDto dto);
    }
}
