using HadiyahDomain.Entities;

namespace HadiyahRepositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllWithRolesAsync();
        Task<User> GetByPhoneAsync(string phoneNumber);
    }
}
