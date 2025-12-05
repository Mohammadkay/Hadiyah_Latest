using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.DTOs.Cart
{
    public class CartItem
    {
        public long ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public long Quantity { get; set; }
        public int StockQuantity { get; set; }
        public string ImageBase64 { get; set; }

        public decimal Total => Price * Quantity;
    }
}
