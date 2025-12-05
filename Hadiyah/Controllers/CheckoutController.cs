using Hadiyah.Helper;
using Hadiyah.Models;
using HadiyahDomain.Entities;
using HadiyahDomain.enums;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hadiyah.Controllers
{
    [Authorize] // checkout requires login
    public class CheckoutController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public CheckoutController(IOrderRepository orderRepository, IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        // GET: /Checkout
        [HttpGet]
        public IActionResult Index()
        {
            var cart = CartSessionHelper.GetCart(HttpContext);
            if (cart == null || !cart.Any())
            {
                TempData["CartMessage"] = "Your cart is empty. Add items before checkout.";
                TempData["CartMessageType"] = "info";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutDto();
            ViewBag.CartItems = cart;
            ViewBag.GrandTotal = cart.Sum(x => x.Total);

            return View(model);
        }

        // POST: /Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CheckoutDto dto)
        {
            var cart = CartSessionHelper.GetCart(HttpContext);
            if (cart == null || !cart.Any())
            {
                TempData["CartMessage"] = "Your cart is empty. Add items before checkout.";
                TempData["CartMessageType"] = "info";
                return RedirectToAction("Index", "Cart");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.CartItems = cart;
                ViewBag.GrandTotal = cart.Sum(x => x.Total);
                return View(dto);
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "User");
            }

            var products = new Dictionary<long, Product>();
            foreach (var item in cart)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    TempData["CartMessage"] = "One of your items is no longer available.";
                    TempData["CartMessageType"] = "danger";
                    return RedirectToAction("Index", "Cart");
                }

                if (product.StockQuantity < item.Quantity)
                {
                    TempData["CartMessage"] = $"{product.Name} only has {product.StockQuantity} in stock. Please adjust your quantity.";
                    TempData["CartMessageType"] = "warning";
                    return RedirectToAction("Index", "Cart");
                }

                products[item.ProductId] = product;
            }

            var total = cart.Sum(x => x.Total);
            var paymentMethod = (dto.PaymentMethod ?? "Cash").Trim();
            var requiresCardPayment = paymentMethod.Equals("Card", StringComparison.OrdinalIgnoreCase);
            var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Valued Customer";
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "support@hadiyah.com";
            var fallbackPhone = "N/A";
            var resolvedName = dto.IsGift ? dto.RecipientName : userName;
            var resolvedEmail = dto.IsGift ? dto.RecipientEmail : userEmail;
            var resolvedPhone = dto.IsGift ? dto.RecipientPhone : fallbackPhone;

            resolvedName = string.IsNullOrWhiteSpace(resolvedName) ? "Valued Customer" : resolvedName;
            resolvedEmail = string.IsNullOrWhiteSpace(resolvedEmail) ? "support@hadiyah.com" : resolvedEmail;
            resolvedPhone = string.IsNullOrWhiteSpace(resolvedPhone) ? "N/A" : resolvedPhone;

            var shippingAddress = dto.ShippingAddress?.Trim() ?? string.Empty;
            if (shippingAddress.Length > 300)
            {
                shippingAddress = shippingAddress.Substring(0, 300);
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = total,
                Status = requiresCardPayment ? OrderStatus.Pending : OrderStatus.Pending,
                IsGift = dto.IsGift,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cart)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = (int)item.Quantity,
                    UnitPrice = item.Price,
                    TotalPrice = item.Total
                });
            }

            order.Recipient = new OrderRecipient
            {
                Name = resolvedName,
                Email = resolvedEmail,
                Phone = resolvedPhone,
                Message = dto.IsGift ? dto.GiftMessage : null,
                ShippingAddress = shippingAddress
            };


            await _orderRepository.AddAsync(order);

            foreach (var item in cart)
            {
                var product = products[item.ProductId];
                product.StockQuantity -= (int)item.Quantity;
                if (product.StockQuantity < 0)
                    product.StockQuantity = 0;

                await _productRepository.UpdateAsync(product);
            }

            CartSessionHelper.ClearCart(HttpContext);

            TempData["OrderSuccess"] = "Your order has been placed successfully!";

            if (requiresCardPayment)
            {
                return RedirectToAction("Payment", new { id = order.Id });
            }

            return RedirectToAction("Confirmation");
        }

        // GET: /Checkout/Confirmation
        [HttpGet]
        public IActionResult Confirmation()
        {
            if (TempData["OrderSuccess"] == null)
            {
                return RedirectToAction("Index", "Shop");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Payment(long id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "User");
            }

            var order = await _orderRepository.GetDetailsAsync(id, userId);
            if (order == null)
            {
                return RedirectToAction("Index", "Orders");
            }

            var vm = new PaymentViewModel
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                OrderDate = order.OrderDate
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Payment(PaymentViewModel model)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
            {
                return RedirectToAction("Login", "User");
            }

            var order = await _orderRepository.GetDetailsAsync(model.OrderId, userId);
            if (order == null)
            {
                return RedirectToAction("Index", "Orders");
            }

            if (!ModelState.IsValid)
            {
                model.Amount = order.TotalAmount;
                model.OrderDate = order.OrderDate;
                return View(model);
            }

            if (order.Status == OrderStatus.Pending)
            {
                order.Status = OrderStatus.Paid;
                await _orderRepository.UpdateAsync(order);
            }

            TempData["OrderSuccess"] = "Payment completed successfully!";
            return RedirectToAction("Confirmation");
        }
    }
}
