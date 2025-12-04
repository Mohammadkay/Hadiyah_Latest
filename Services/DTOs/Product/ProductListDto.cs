using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.DTOs.Product
{
    public class ProductListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageBase64 { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
