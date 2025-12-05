using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HadiyahServices.DTOs.Category
{
    public class CategoryCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public IFormFile imageFile { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
