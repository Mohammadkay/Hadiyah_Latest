using System;
using System.Linq;
using HadiyahDomain.Entities;
using HadiyahDomain.enums;
using HadiyahMigrations;
using HadiyahRepositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HadiyahRepositories.Implementation
{
    public class OtpRepository : Repository<Otp>, IOtpRepository
    {
        public OtpRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task InvalidateActiveCodesAsync(long userId, OtpType otpType)
        {
            var now = DateTime.UtcNow;
            var activeCodes = await _dbSet
                .Where(o => o.UserId == userId && o.OtpType == otpType && !o.IsUsed && o.ExpiresAt > now)
                .ToListAsync();

            if (activeCodes.Count == 0)
                return;

            foreach (var otp in activeCodes)
            {
                otp.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Otp?> GetValidCodeAsync(long userId, string code, OtpType otpType)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(o => o.UserId == userId && o.OtpType == otpType && o.Code == code)
                .Where(o => !o.IsUsed && o.ExpiresAt > now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
