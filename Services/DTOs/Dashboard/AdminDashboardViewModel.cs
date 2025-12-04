using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.DTOs.Dashboard
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal Revenue { get; set; }
    }
}
