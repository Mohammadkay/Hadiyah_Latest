using Hadiyah.Attributes;
using Hadiyah.Models;
using HadiyahDomain.enums;
using HadiyahRepositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Hadiyah.Controllers
{
    [AdminAuthorize]
    public class AdminOrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public AdminOrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetOrdersWithItemsAsync();
            var list = orders
                .OrderByDescending(o => o.OrderDate)
                .Select(o =>
                {
                    var expected = o.OrderDate.AddDays(3);
                    var customerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}" : "Customer";
                    var email = o.User?.Email ?? o.Recipient?.Email ?? "N/A";
                    var recipient = o.Recipient?.Name ?? (o.IsGift ? "Gift recipient" : customerName);
                    var isTerminal = o.Status == OrderStatus.Completed || o.Status == OrderStatus.Cancelled;
                    return new AdminOrderListItemViewModel
                    {
                        Id = o.Id,
                        CustomerName = customerName,
                        CustomerEmail = email,
                        RecipientName = recipient,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        IsGift = o.IsGift,
                        OrderDate = o.OrderDate,
                        ExpectedDeliveryDate = expected,
                        IsOverdue = !isTerminal && DateTime.UtcNow > expected
                    };
                })
                .ToList();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(long id, OrderStatus status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                TempData["AdminOrderMessage"] = "Order not found.";
                return RedirectToAction("Index");
            }

            order.Status = status;
            await _orderRepository.UpdateAsync(order);
            TempData["AdminOrderMessage"] = $"Order #{order.Id} updated to {status}.";
            return RedirectToAction("Index");
        }
    }
}
