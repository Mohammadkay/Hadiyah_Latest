using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahDomain.Entities
{
    public class OrderRecipient
    {
        [Key]
        public long Id { get; set; }

        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        [StringLength(300)]
        public string? ShippingAddress { get; set; }

        public long OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}
