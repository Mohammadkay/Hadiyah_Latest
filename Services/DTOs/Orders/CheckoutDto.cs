using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HadiyahServices.DTOs.Orders
{
    public class CheckoutDto : IValidatableObject
    {
        [Required(ErrorMessage = "Shipping address is required.")]
        [StringLength(250, ErrorMessage = "Shipping address cannot exceed 250 characters.")]
        public string ShippingAddress { get; set; } = string.Empty;

        public bool IsGift { get; set; }

        [MaxLength(120)]
        public string? RecipientName { get; set; }

        [EmailAddress(ErrorMessage = "Please enter a valid recipient email.")]
        public string? RecipientEmail { get; set; }

        [Phone(ErrorMessage = "Please enter a valid recipient phone number.")]
        public string? RecipientPhone { get; set; }

        [MaxLength(500, ErrorMessage = "Gift message is too long.")]
        public string? GiftMessage { get; set; }

        [Required(ErrorMessage = "Select a payment method.")]
        public string PaymentMethod { get; set; } = "Cash";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsGift)
            {
                if (string.IsNullOrWhiteSpace(RecipientName))
                {
                    yield return new ValidationResult(
                        "Recipient name is required when sending a gift.",
                        new[] { nameof(RecipientName) });
                }

                if (string.IsNullOrWhiteSpace(RecipientEmail))
                {
                    yield return new ValidationResult(
                        "Recipient email is required when sending a gift.",
                        new[] { nameof(RecipientEmail) });
                }
            }

            var method = (PaymentMethod ?? string.Empty).Trim().ToLowerInvariant();
            if (method != "cash" && method != "card")
            {
                yield return new ValidationResult(
                    "Invalid payment method selected.",
                    new[] { nameof(PaymentMethod) });
            }
        }
    }
}
