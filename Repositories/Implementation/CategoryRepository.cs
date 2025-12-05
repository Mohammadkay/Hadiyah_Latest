using HadiyahDomain.Entities;
using HadiyahMigrations;
using HadiyahRepositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HadiyahRepositories.Implementation
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }
    }
}
