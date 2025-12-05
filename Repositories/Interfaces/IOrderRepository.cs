using HadiyahDomain.Entities;
using HadiyahDomain.enums;

namespace HadiyahRepositories.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersWithItemsAsync();
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<List<Order>> GetByUserIdAsync(long userId);
        Task<Order?> GetDetailsAsync(long orderId, long userId);
    }
}
