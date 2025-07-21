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

        public async Task<(bool Success, string? ErrorMessage)> ImportProductAsync(IEnumerable<object> produtos, string jwtToken)
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

        public async Task<(bool Success, string? ErrorMessage)> ImportInventAsync(IEnumerable<object> invents, string jwtToken)
        {
            var url = $"{_settings.BaseUrl}{ApiEndpoints.Product.Invent}";

            var json = JsonSerializer.Serialize(invents);
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

        public async Task<(bool Success, string? ErrorMessage)> ImportOxfordAsync(IEnumerable<object> oxfords, string jwtToken)
        {
            var url = $"{_settings.BaseUrl}{ApiEndpoints.Product.Oxford}";

            var json = JsonSerializer.Serialize(oxfords);
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

        public async Task<(bool Success, string? ErrorMessage)> ImportTaxInformationAsync(IEnumerable<object> taxInformations, string jwtToken)
        {
            var url = $"{_settings.BaseUrl}{ApiEndpoints.Product.TaxInformation}";

            var json = JsonSerializer.Serialize(taxInformations);
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
