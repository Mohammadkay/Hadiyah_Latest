using System.ComponentModel.DataAnnotations;

namespace HadiyahServices.DTOs.Category
{
    public class CategoryUpdateDto
    {
        public long Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? ImageBase64 { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
    }
}
