using Domain.Entities;
using HadiyahServices.DTOs.enums;
using HadiyahServices.DTOs.User;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Hadiyah.Controllers
{
    public class UserController : Controller
    {
        private readonly IAuthService _authService;

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
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
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
                Expires = DateTime.UtcNow.AddHours(1)
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

            return RedirectToAction("Index", "Shop");
        }



        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return RedirectToAction("Login");
        }

    }
}
