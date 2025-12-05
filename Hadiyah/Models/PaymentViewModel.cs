using System;
using System.ComponentModel.DataAnnotations;

namespace Hadiyah.Models
{
    public class PaymentViewModel
    {
        public long OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime OrderDate { get; set; }

        [Required]
        [Display(Name = "Name on card")]
        public string CardholderName { get; set; }

        [Required]
        [Display(Name = "Card number")]
        [RegularExpression(@"^\d{4}( \d{4}){3}$", ErrorMessage = "Enter 16 digits separated by spaces every 4 digits.")]
        public string CardNumber { get; set; }

        [Required]
        [Range(0, 12, ErrorMessage = "Month must be between 0 and 12.")]
        [Display(Name = "Expiry month")]
        public int ExpiryMonth { get; set; }

        [Required]
        [Display(Name = "Expiry year (YY)")]
        [RegularExpression(@"^\d{2}$", ErrorMessage = "Use two digits, e.g. 28 for 2028.")]
        public string ExpiryYear { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 3)]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        [Display(Name = "CVV")]
        public string Cvv { get; set; }
    }
}
