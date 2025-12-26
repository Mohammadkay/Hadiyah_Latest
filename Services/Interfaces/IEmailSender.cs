using System.Threading.Tasks;

namespace HadiyahServices.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string body);
    }
}
