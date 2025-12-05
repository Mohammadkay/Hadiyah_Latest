using HadiyahServices.DTOs.Cart;
using System.Text.Json;

namespace Hadiyah.Helper
{
    public static class CartSessionHelper
    {
        private const string CartKey = "Cart";

        public static List<CartItem> GetCart(HttpContext httpContext)
        {
            var json = httpContext.Session.GetString(CartKey);
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();

            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public static void SaveCart(HttpContext httpContext, List<CartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            httpContext.Session.SetString(CartKey, json);
        }

        public static void ClearCart(HttpContext httpContext)
        {
            httpContext.Session.Remove(CartKey);
        }
    }
}
