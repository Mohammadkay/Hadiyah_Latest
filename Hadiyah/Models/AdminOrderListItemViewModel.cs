using HadiyahDomain.enums;
using System;

namespace Hadiyah.Models
{
    public class AdminOrderListItemViewModel
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string RecipientName { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public bool IsGift { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedDeliveryDate { get; set; }
        public bool IsOverdue { get; set; }
    }
}
