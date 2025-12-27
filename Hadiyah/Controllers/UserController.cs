using Hadiyah.Models;
using HadiyahDomain.Entities;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.enums;
using HadiyahServices.DTOs.User;
using HadiyahServices.Implementation;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Hadiyah.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly TokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;
        private const string ResetEmailSessionKey = "ResetEmail";
        private const string ResetCodeSessionKey = "ResetCode";

        public UserController(IAuthService authService, IUserRepository userRepository, TokenService tokenService)
        {
            _authService = authService;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = new PasswordHasher<User>();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.Register(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(dto);
            }

            TempData["Success"] = "Registration completed! Please login.";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Login(string? returnUrl = null, string? reason = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!string.IsNullOrWhiteSpace(reason))
            {
                var message = reason == "forbidden"
                    ? "You don't have permission to access that page. Please login with the correct account."
                    : "Please login to continue.";
                TempData["Alert.Type"] = "warning";
                TempData["Alert.Title"] = "Authentication required";
                TempData["Alert.Message"] = message;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.Login(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(dto);
            }

            // Store token in cookie
            Response.Cookies.Append("jwt", result.Data, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            });

            // get user role from token claims
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(result.Data);
            var roleIdClaim = token.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;

            int.TryParse(roleIdClaim, out int roleId);

            if (roleId == (int)UserRole.Admin)
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Shop");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = BuildProfileViewModel(user);
            return View(model);
        }

        [Authorize]
        public IActionResult Logout()
        {
            HttpContext.Session?.Clear();
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied(string? message = null)
        {
            TempData["Alert.Type"] = "error";
            TempData["Alert.Title"] = "Access denied";
            TempData["Alert.Message"] = message ?? "You are not authorized to view that page.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var normalizedEmail = email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return View();
            }

            var result = await _authService.ForgotPassword(normalizedEmail);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View();
            }

            HttpContext.Session.SetString(ResetEmailSessionKey, normalizedEmail);
            HttpContext.Session.Remove(ResetCodeSessionKey);
            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString(ResetEmailSessionKey)))
                return RedirectToAction("ForgotPassword");

            return View(new VerifyOtpDto());
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var email = HttpContext.Session.GetString(ResetEmailSessionKey);
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Session expired. Please start again.");
                return View(dto);
            }

            var result = await _authService.VerifyResetCode(email, dto.Code);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(dto);
            }

            HttpContext.Session.SetString(ResetCodeSessionKey, dto.Code.Trim());
            return RedirectToAction("ResetPassword");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = HttpContext.Session.GetString(ResetEmailSessionKey);
            var code = HttpContext.Session.GetString(ResetCodeSessionKey);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                return RedirectToAction("ForgotPassword");

            return View(new ResetPasswordFormDto());
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var email = HttpContext.Session.GetString(ResetEmailSessionKey);
            var code = HttpContext.Session.GetString(ResetCodeSessionKey);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError(string.Empty, "Session expired. Please start again.");
                return View(dto);
            }

            var resetDto = new ResetPasswordDto
            {
                Email = email,
                Code = code,
                Password = dto.Password,
                ConfirmPassword = dto.ConfirmPassword
            };

            var result = await _authService.ResetPassword(resetDto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(dto);
            }

            HttpContext.Session.Remove(ResetEmailSessionKey);
            HttpContext.Session.Remove(ResetCodeSessionKey);

            TempData["Success"] = "Password reset successful. Please login.";
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind(Prefix = "Profile")] ProfileUpdateViewModel profile)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            profile.Email = profile.Email?.Trim().ToLowerInvariant();
            profile.PhoneNumber = profile.PhoneNumber?.Trim();
            if (string.IsNullOrWhiteSpace(profile.PhoneNumber))
            {
                profile.PhoneNumber = null;
            }

            ModelState.Clear();
            TryValidateModel(profile, "Profile");

            if (!ModelState.IsValid)
            {
                return View("Profile", BuildProfileViewModel(user, profile, new ChangePasswordViewModel()));
            }

            var existingUser = await _userRepository.GetByEmailAsync(profile.Email);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                ModelState.AddModelError("Profile.Email", "Email already exists.");
            }

            if (!string.IsNullOrWhiteSpace(profile.PhoneNumber))
            {
                var existingPhone = await _userRepository.GetByPhoneAsync(profile.PhoneNumber);
                if (existingPhone != null && existingPhone.Id != user.Id)
                {
                    ModelState.AddModelError("Profile.PhoneNumber", "Phone number already exists.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View("Profile", BuildProfileViewModel(user, profile, new ChangePasswordViewModel()));
            }

            user.Email = profile.Email;
            user.PhoneNumber = profile.PhoneNumber;
            await _userRepository.UpdateAsync(user);

            await RefreshUserTokenAsync(user.Email);

            TempData["ProfileMessage"] = "Profile updated successfully.";
            TempData["ProfileMessageType"] = "success";
            return RedirectToAction("Profile");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangePasswordViewModel password)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View("Profile", BuildProfileViewModel(user, null, password));
            }

            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password.CurrentPassword);
            if (verification != PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("Password.CurrentPassword", "Current password is incorrect.");
                return View("Profile", BuildProfileViewModel(user, null, password));
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, password.NewPassword);
            await _userRepository.UpdateAsync(user);

            TempData["ProfileMessage"] = "Password updated successfully.";
            TempData["ProfileMessageType"] = "success";
            return RedirectToAction("Profile");
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return await _userRepository.GetByIdAsync(userId);
        }

        private UserProfileViewModel BuildProfileViewModel(User user, ProfileUpdateViewModel? profile = null, ChangePasswordViewModel? password = null)
        {
            var displayName = $"{user.FirstName} {user.LastName}".Trim();

            return new UserProfileViewModel
            {
                DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Valued Customer" : displayName,
                Profile = profile ?? new ProfileUpdateViewModel
                {
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                },
                Password = password ?? new ChangePasswordViewModel()
            };
        }

        private async Task RefreshUserTokenAsync(string email)
        {
            var updatedUser = await _userRepository.GetByEmailAsync(email);
            if (updatedUser?.Role == null)
            {
                return;
            }

            var token = _tokenService.GenerateToken(updatedUser);
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                IsEssential = true
            });
        }
    }
}
