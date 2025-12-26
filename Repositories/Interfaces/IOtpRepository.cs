using System.Threading.Tasks;
using HadiyahDomain.Entities;
using HadiyahDomain.enums;

namespace HadiyahRepositories.Interfaces
{
    public interface IOtpRepository : IRepository<Otp>
    {
        Task InvalidateActiveCodesAsync(long userId, OtpType otpType);
        Task<Otp?> GetValidCodeAsync(long userId, string code, OtpType otpType);
    }
}
