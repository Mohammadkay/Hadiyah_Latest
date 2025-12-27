using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.DTOs.User
{
    public class RegisterDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 7, ErrorMessage = "Password must be at least 7 characters.")]
        [RegularExpression("^(?=.*[A-Z])(?=.*[^a-zA-Z0-9]).{7,}$", ErrorMessage = "Password must include at least one uppercase letter and one special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [RegularExpression("^$|^07\\d{8}$", ErrorMessage = "Phone number must start with 07 and be 10 digits.")]
        public string? PhoneNumber { get; set; }
    }
}
