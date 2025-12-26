using HadiyahServices.DTOs.enums;
using HadiyahServices.DTOs.User;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Hadiyah.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;
        private const string ResetEmailSessionKey = "ResetEmail";
        private const string ResetCodeSessionKey = "ResetCode";

        public UserController(IAuthService authService)
        {
            _authService = authService;
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
        public IActionResult Profile()
        {
            return View();
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

    }
}
