using Domain.Entities;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Common;
using HadiyahServices.DTOs.User;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace HadiyahServices.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly TokenService _tokenService;

        public AuthService(IUserRepository userRepo, TokenService tokenService)
        {
            _userRepo = userRepo;
            _passwordHasher = new PasswordHasher<User>();
            _tokenService = tokenService;
        }

        public async Task<BaseResponse<string>> Register(RegisterDto dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                return BaseResponse<string>.Fail("Passwords do not match");

            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user != null)
                return BaseResponse<string>.Fail("Email already exists");

            var newUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                RoleId = 2
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, dto.Password);

            await _userRepo.AddAsync(newUser);

            return BaseResponse<string>.Success("Registration completed");
        }

        public async Task<BaseResponse<string>> Login(LoginDto dto)
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);

            if (user == null || !user.IsActive)
                return BaseResponse<string>.Fail("Invalid email or password");

            var check = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (check != PasswordVerificationResult.Success)
                return BaseResponse<string>.Fail("Invalid email or password");

            var token = _tokenService.GenerateToken(user);

            return BaseResponse<string>.Success(token);
        }
    }
}
