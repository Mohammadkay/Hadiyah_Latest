using System.ComponentModel.DataAnnotations;

namespace Hadiyah.Models
{
    public class AdminChangePasswordViewModel
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
