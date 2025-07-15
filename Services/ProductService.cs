using Microsoft.Extensions.Options;
using OXF.Constants;
using OXF.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OXF.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _settings;

        public ProductService(HttpClient httpClient, IOptions<ApiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<(bool Success, string? ErrorMessage)> ImportAsync(IEnumerable<object> produtos, string jwtToken)
        {
            var url = $"{_settings.BaseUrl}{ApiEndpoints.Product.Products}";

            var json = JsonSerializer.Serialize(produtos);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
                return (true, null);

            var errorText = await response.Content.ReadAsStringAsync();
            return (false, Messages.ErrorProcess($"{response.StatusCode} - {errorText}"));
        }
    }
}
