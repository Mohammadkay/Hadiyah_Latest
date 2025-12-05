using HadiyahDomain.Entities;

namespace HadiyahRepositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<bool> ExistsByNameAsync(string name);
    }
}
