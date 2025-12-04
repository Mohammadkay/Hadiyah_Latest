using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.DTOs.Product
{
    public class ProductUpdateDto
    {
        public long Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageBase64 { get; set; }

        public int StockQuantity { get; set; }

        public bool IsActive { get; set; }

        public long CategoryId { get; set; }
    }
}
