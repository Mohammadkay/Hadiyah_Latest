using System.Security.Claims;
using HadiyahRepositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hadiyah.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // GET: /Orders
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "User");
            }

            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return View(orders);
        }

        // GET: /Orders/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "User");
            }

            var order = await _orderRepository.GetDetailsAsync(id, userId);
            if (order == null)
                return RedirectToAction("Index");

            return View(order);
        }
    }
}
