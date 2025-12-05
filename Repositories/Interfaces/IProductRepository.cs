using HadiyahDomain.Entities;

namespace HadiyahRepositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId);
        Task<bool> ExistsByNameAsync(string name);
    }
}
