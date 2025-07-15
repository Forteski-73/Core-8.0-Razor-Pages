using Microsoft.Extensions.Options;
using OXF.Constants;
using OXF.Models;

namespace OXF.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _settings;

        public AuthService(HttpClient httpClient, IOptions<ApiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var url = $"{_settings.BaseUrl}{ApiEndpoints.User.Login}";

            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.TryAddWithoutValidation("Authorization", _settings.FixedToken);
            request.Content = JsonContent.Create(new { User = username, Password = password });

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            return result is not null && result.TryGetValue("token", out var token) ? token : null;
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(ApiUser user)
        {
            var url = $"{_settings.BaseUrl}{ApiEndpoints.User.Register}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(user)
            };
            request.Headers.TryAddWithoutValidation("Authorization", _settings.FixedToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return (true, Messages.api.Success);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                return (false, Messages.api.UserExists);

            var errorText = await response.Content.ReadAsStringAsync();
            return (false, Messages.ErrorProcess(errorText));
        }
    }
}
