using System;
using System.Text.RegularExpressions;
using HadiyahDomain.Entities;
using HadiyahDomain.enums;
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
        private readonly IOtpRepository _otpRepository;
        private readonly IEmailSender _emailSender;

        private const string PasswordPattern = "^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{7,}$";

        public AuthService(
            IUserRepository userRepo,
            TokenService tokenService,
            IOtpRepository otpRepository,
            IEmailSender emailSender)
        {
            _userRepo = userRepo;
            _passwordHasher = new PasswordHasher<User>();
            _tokenService = tokenService;
            _otpRepository = otpRepository;
            _emailSender = emailSender;
        }

        public async Task<BaseResponse<string>> Register(RegisterDto dto)
        {
            var phone = dto.PhoneNumber?.Trim();
            var email = dto.Email?.Trim().ToLowerInvariant();

            if (!Regex.IsMatch(dto.Password ?? string.Empty, PasswordPattern))
                return BaseResponse<string>.Fail("Password must be at least 7 characters with one uppercase and one special character.");

            if (dto.Password != dto.ConfirmPassword)
                return BaseResponse<string>.Fail("Passwords do not match");

            var user = await _userRepo.GetByEmailAsync(email);
            if (user != null)
                return BaseResponse<string>.Fail("Email already exists");

            var phoneUser = await _userRepo.GetByPhoneAsync(phone);
            if (phoneUser != null)
                return BaseResponse<string>.Fail("Phone number already exists");

            var newUser = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = email,
                PhoneNumber = phone,
                RoleId = 2
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, dto.Password);

            await _userRepo.AddAsync(newUser);

            return BaseResponse<string>.Success("Registration completed");
        }

        public async Task<BaseResponse<string>> Login(LoginDto dto)
        {
            var email = dto.Email?.Trim().ToLowerInvariant();
            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null || !user.IsActive)
                return BaseResponse<string>.Fail("Invalid email or password");

            var check = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (check != PasswordVerificationResult.Success)
                return BaseResponse<string>.Fail("Invalid email or password");

            var token = _tokenService.GenerateToken(user);

            return BaseResponse<string>.Success(token);
        }

        public async Task<BaseResponse<string>> ForgotPassword(string email)
        {
            var normalizedEmail = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return BaseResponse<string>.Fail("Email is required.");

            var user = await _userRepo.GetByEmailAsync(normalizedEmail);
            if (user == null || !user.IsActive)
                return BaseResponse<string>.Success("If the email exists, a reset code has been sent.");

            try
            {
                var code = GenerateCode();

                await _otpRepository.InvalidateActiveCodesAsync(user.Id, OtpType.ForgetPassword);

                var otp = new Otp
                {
                    UserId = user.Id,
                    Purpose = "password-reset",
                    Code = code,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                    CreatedAt = DateTime.UtcNow,
                    OtpType = OtpType.ForgetPassword,
                    IsUsed = false
                };
                await _otpRepository.AddAsync(otp);

                var body = $@"<p>Hello {user.FirstName},</p>
                              <p>Your password reset code is: <strong>{code}</strong></p>
                              <p>This code expires in 5 minutes.</p>";
                await _emailSender.SendAsync(normalizedEmail, "Password Reset Code", body);
            }
            catch
            {
                return BaseResponse<string>.Fail("Unable to send reset code. Please try again later.");
            }

            return BaseResponse<string>.Success("If the email exists, a reset code has been sent.");
        }

        public async Task<BaseResponse<string>> VerifyResetCode(string email, string code)
        {
            var normalizedEmail = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return BaseResponse<string>.Fail("Email is required.");

            if (string.IsNullOrWhiteSpace(code))
                return BaseResponse<string>.Fail("Code is required.");

            var user = await _userRepo.GetByEmailAsync(normalizedEmail);
            if (user == null || !user.IsActive)
                return BaseResponse<string>.Fail("Invalid or expired code.");

            var trimmedCode = code.Trim();
            var otp = await _otpRepository.GetValidCodeAsync(user.Id, trimmedCode, OtpType.ForgetPassword);
            if (otp == null)
                return BaseResponse<string>.Fail("Invalid or expired code.");

            return BaseResponse<string>.Success("Code verified.");
        }

        public async Task<BaseResponse<string>> ResetPassword(ResetPasswordDto dto)
        {
            var normalizedEmail = dto.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return BaseResponse<string>.Fail("Email is required.");

            var user = await _userRepo.GetByEmailAsync(normalizedEmail);
            if (user == null || !user.IsActive)
                return BaseResponse<string>.Fail("Invalid code or email.");

            try
            {
                var trimmedCode = dto.Code?.Trim();
                var otp = await _otpRepository.GetValidCodeAsync(user.Id, trimmedCode, OtpType.ForgetPassword);
                if (otp == null)
                    return BaseResponse<string>.Fail("Invalid or expired code.");

                if (!Regex.IsMatch(dto.Password ?? string.Empty, PasswordPattern))
                    return BaseResponse<string>.Fail("Password must be at least 7 characters with one uppercase and one special character.");

                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
                await _userRepo.UpdateAsync(user);

                otp.IsUsed = true;
                await _otpRepository.UpdateAsync(otp);
            }
            catch
            {
                return BaseResponse<string>.Fail("Unable to reset password. Please try again later.");
            }

            return BaseResponse<string>.Success("Password has been reset. You can now log in.");
        }

        private static string GenerateCode()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }
    }
}
