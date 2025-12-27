using System.ComponentModel.DataAnnotations;

namespace Hadiyah.Models
{
    public class UserProfileViewModel
    {
        public string DisplayName { get; set; } = string.Empty;
        public ProfileUpdateViewModel Profile { get; set; } = new ProfileUpdateViewModel();
        public ChangePasswordViewModel Password { get; set; } = new ChangePasswordViewModel();
    }

    public class ProfileUpdateViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [RegularExpression("^$|^07\\d{8}$", ErrorMessage = "Phone number must start with 07 and be 10 digits.")]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Password must be at least 7 characters.")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{7,}$", ErrorMessage = "Password must include at least one uppercase letter and one special character.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required]
        [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }
}
