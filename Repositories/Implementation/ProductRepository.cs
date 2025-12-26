using HadiyahDomain.Entities;
using HadiyahMigrations;
using HadiyahRepositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HadiyahRepositories.Implementation
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId)
        {
            return await GetFilteredAsync(categoryId, null, null);
        }

        public async Task<List<Product>> GetFilteredAsync(long? categoryId, decimal? minPrice, decimal? maxPrice, int? skip = null, int? take = null)
        {
            return await GetAsync(
                predicate: p =>
                    (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
                    (!minPrice.HasValue || p.Price >= minPrice.Value) &&
                    (!maxPrice.HasValue || p.Price <= maxPrice.Value),
                orderBy: q => q.OrderByDescending(p => p.CreatedAt),
                include: q => q.Include(p => p.Category),
                disableTracking: true,
                skip: skip,
                take: take);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Products
                .AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }
    }
}
