using HadiyahDomain.Entities;

namespace HadiyahRepositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId);
        Task<List<Product>> GetFilteredAsync(long? categoryId, decimal? minPrice, decimal? maxPrice, int? skip = null, int? take = null);
        Task<bool> ExistsByNameAsync(string name);
    }
}
