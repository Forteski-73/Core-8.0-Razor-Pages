using Microsoft.Extensions.Options;
using OXF.Constants;
using OXF.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace OXF.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _settings;

        public InventoryService(HttpClient httpClient, IOptions<ApiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        /// <summary>
        /// Busca todos os inventários resumidos
        /// </summary>
        public async Task<List<Inventory>> GetAllAsync(string jwtToken)
        {
            var url = $"{_settings.BaseUrl}/v1/Inventory/All";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Inventory>>() ?? new();
            }

            return new List<Inventory>();
        }

        /// <summary>
        /// Busca os detalhes (records) de um inventário específico via GUID
        /// </summary>
        public async Task<List<InventoryRecord>> GetRecordsByGuidAsync(string code, string jwtToken)
        {
            var url = $"{_settings.BaseUrl}/v1/Inventory/Record/ByCode/{code}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return new List<InventoryRecord>();

            var data = await response.Content.ReadFromJsonAsync<List<InventoryRecord>>();

            return data ?? new List<InventoryRecord>();
        }
    }
}