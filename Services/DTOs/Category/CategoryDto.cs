using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HadiyahServices.DTOs.Category
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string ImageBase64 { get; set; }
        public IFormFile ImageFile { get; set; }
        public int ProductCount { get; set; }
    }
}
