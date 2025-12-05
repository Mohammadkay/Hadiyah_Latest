using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahRepositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryAsync(long categoryId);
        Task<bool> ExistsByNameAsync(string name);
    }
}
