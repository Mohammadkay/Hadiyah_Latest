using System.ComponentModel.DataAnnotations;

namespace HadiyahServices.DTOs.User
{
    public class ResetPasswordFormDto
    {
        [Required]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Password must be at least 7 characters.")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{7,}$", ErrorMessage = "Password must include at least one uppercase letter and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
