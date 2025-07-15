using OXF.Models;

namespace OXF.Services
{
    public interface IAuthService
    {
        Task<string?> AuthenticateAsync(string username, string password);

        Task<(bool Success, string ErrorMessage)> RegisterAsync(ApiUser user);
    }
}
