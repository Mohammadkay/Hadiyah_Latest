using HadiyahDomain.enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace HadiyahDomain.Entities
{
    public class Otp
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; }

        public long? UserId { get; set; }

        [StringLength(50)]
        public string? Purpose { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsUsed { get; set; } = false;
        public OtpType OtpType { get; set; }
    }
}
