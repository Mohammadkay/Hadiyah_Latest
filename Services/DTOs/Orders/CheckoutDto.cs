using System;
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

        [DataType(DataType.Date)]
        public DateTime? GiftArrivalDate { get; set; }

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

                if (!GiftArrivalDate.HasValue)
                {
                    yield return new ValidationResult(
                        "Gift arrival date is required when sending a gift.",
                        new[] { nameof(GiftArrivalDate) });
                }
                else if (GiftArrivalDate.Value.Date < DateTime.UtcNow.Date.AddDays(2))
                {
                    yield return new ValidationResult(
                        "Gift arrival date must be at least 2 days from today.",
                        new[] { nameof(GiftArrivalDate) });
                }
            }

            var method = (PaymentMethod ?? string.Empty).Trim().ToLowerInvariant();
            if (IsGift && method != "card")
            {
                yield return new ValidationResult(
                    "Gift orders must be paid online.",
                    new[] { nameof(PaymentMethod) });
            }
            if (method != "cash" && method != "card")
            {
                yield return new ValidationResult(
                    "Invalid payment method selected.",
                    new[] { nameof(PaymentMethod) });
            }
        }
    }
}
