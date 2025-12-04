using Domain.Entities;
using HadiyahDomain.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahRepositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersWithItemsAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    }
}
