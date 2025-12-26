using System.ComponentModel.DataAnnotations;

namespace HadiyahServices.DTOs.User
{
    public class VerifyOtpDto
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 characters.")]
        public string Code { get; set; }
    }
}
