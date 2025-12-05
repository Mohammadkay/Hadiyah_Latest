using Hadiyah.Attributes;
using Hadiyah.Models;
using HadiyahDomain.Entities;
using HadiyahRepositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Hadiyah.Controllers
{
    [AdminAuthorize]
    public class AdminUsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;

        public AdminUsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<IActionResult> Index(string? search)
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            var keyword = search?.Trim();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowered = keyword.ToLowerInvariant();
                users = users.Where(u =>
                    u.FirstName.ToLower().Contains(lowered) ||
                    u.LastName.ToLower().Contains(lowered) ||
                    u.Email.ToLower().Contains(lowered)).ToList();
            }

            ViewBag.Search = keyword;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userRepository.UpdateAsync(user);
                TempData["AdminUserMessage"] = $"{user.FirstName} {(user.IsActive ? "activated" : "paused")}.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                TempData["AdminUserMessage"] = "User not found.";
                TempData["AdminUserMessageType"] = "danger";
                return RedirectToAction("Index");
            }

            var vm = new AdminChangePasswordViewModel
            {
                UserId = user.Id,
                DisplayName = $"{user.FirstName} {user.LastName}"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepository.GetByIdAsync(model.UserId);
            if (user == null)
            {
                TempData["AdminUserMessage"] = "User not found.";
                TempData["AdminUserMessageType"] = "danger";
                return RedirectToAction("Index");
            }

            user.PasswordHash = _passwordHasher.HashPassword(user, model.NewPassword);
            await _userRepository.UpdateAsync(user);
            TempData["AdminUserMessage"] = $"Password reset for {user.FirstName} {user.LastName}.";
            TempData["AdminUserMessageType"] = "success";
            return RedirectToAction("Index");
        }
    }
}
