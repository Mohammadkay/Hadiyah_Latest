using Hadiyah.Helper;
using Hadiyah.Models;
using HadiyahServices.DTOs.Cart;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hadiyah.Controllers
{
    [AllowAnonymous] // for now cart can be used without login
    public class CartController : Controller
    {
        private readonly IProductService _productService;

        public CartController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = CartSessionHelper.GetCart(HttpContext);
            return View(cart);
        }

        // POST: /Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(long productId, int quantity = 1)
        {
            if (quantity < 1)
                quantity = 1;

            var productResult = await _productService.GetByIdAsync(productId);
            if (!productResult.IsSuccess || productResult.Data == null)
            {
                TempData["CartMessage"] = "Unable to find that product.";
                TempData["CartMessageType"] = "danger";
                return RedirectToAction("Index", "Shop");
            }

            var product = productResult.Data;
            if (product.StockQuantity <= 0)
            {
                TempData["CartMessage"] = $"{product.Name} is currently out of stock.";
                TempData["CartMessageType"] = "warning";
                return RedirectToAction("ProductDetails", "Shop", new { id = productId });
            }

            var cart = CartSessionHelper.GetCart(HttpContext);
            var existing = cart.FirstOrDefault(c => c.ProductId == productId);
            var existingQuantity = existing?.Quantity ?? 0;
            var availableToAdd = product.StockQuantity - (int)existingQuantity;

            if (availableToAdd <= 0)
            {
                TempData["CartMessage"] = $"You already have the maximum available quantity of {product.Name} in your cart.";
                TempData["CartMessageType"] = "info";
                return RedirectToAction("Index");
            }

            var quantityToAdd = Math.Min(quantity, availableToAdd);

            if (existing != null)
            {
                existing.Quantity += quantityToAdd;
                existing.StockQuantity = product.StockQuantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantityToAdd,
                    StockQuantity = product.StockQuantity,
                    ImagePath = product.ImagePath
                });
            }

            CartSessionHelper.SaveCart(HttpContext, cart);

            if (quantityToAdd < quantity)
            {
                TempData["CartMessage"] = $"Only {quantityToAdd} {(quantityToAdd == 1 ? "item was" : "items were")} added because stock is limited.";
                TempData["CartMessageType"] = "warning";
            }
            else
            {
                TempData["CartMessage"] = $"{product.Name} added to your cart.";
                TempData["CartMessageType"] = "success";
            }

            return RedirectToAction("Index");
        }


        // GET: /Cart/Remove/5
        public IActionResult Remove(long productId)
        {
            var cart = CartSessionHelper.GetCart(HttpContext);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                CartSessionHelper.SaveCart(HttpContext, cart);
            }

            return RedirectToAction("Index");
        }

        // GET: /Cart/Clear
        public IActionResult Clear()
        {
            CartSessionHelper.ClearCart(HttpContext);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(long productId, int delta)
        {
            var cart = CartSessionHelper.GetCart(HttpContext);
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item != null)
            {
                var productResult = await _productService.GetByIdAsync(productId);
                if (!productResult.IsSuccess || productResult.Data == null)
                {
                    cart.Remove(item);
                    CartSessionHelper.SaveCart(HttpContext, cart);
                    TempData["CartMessage"] = "That product is no longer available.";
                    TempData["CartMessageType"] = "danger";
                    return RedirectToAction("Index");
                }

                var stock = productResult.Data.StockQuantity;
                item.StockQuantity = stock;

                if (stock <= 0)
                {
                    cart.Remove(item);
                    CartSessionHelper.SaveCart(HttpContext, cart);
                    TempData["CartMessage"] = $"{item.Name} is out of stock and was removed from your cart.";
                    TempData["CartMessageType"] = "warning";
                    return RedirectToAction("Index");
                }

                var newQuantity = item.Quantity + delta;

                if (newQuantity <= 0)
                {
                    cart.Remove(item);
                    TempData["CartMessage"] = $"{item.Name} removed from your cart.";
                    TempData["CartMessageType"] = "info";
                }
                else if (newQuantity > stock)
                {
                    item.Quantity = stock;
                    TempData["CartMessage"] = $"Only {stock} item(s) of {item.Name} are available.";
                    TempData["CartMessageType"] = "warning";
                }
                else
                {
                    item.Quantity = newQuantity;
                }

                CartSessionHelper.SaveCart(HttpContext, cart);
            }

            return RedirectToAction("Index");
        }

    }
}
