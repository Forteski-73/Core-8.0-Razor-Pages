using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Constants;
using OXF.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OXF.Pages
{
    [Authorize]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ProductModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public List<Product> Product { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToPage("/Login");

            try
            {
                var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage = Messages.import.InvalidSession;
                    return RedirectToPage("/Login");
                }

                var baseUrl = _configuration["ApiSettings:BaseUrl"];
                var url = $"{baseUrl}{ApiEndpoints.Product.Products}";

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"{Messages.product.ErrorSelectProduct} {response.StatusCode}";
                    return Page();
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                Product = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new();
            }
            catch (HttpRequestException httpEx)
            {
                ErrorMessage = $"{Messages.api.APIConnectError} {httpEx.Message}";
            }
            catch (JsonException jsonEx)
            {
                ErrorMessage = $"{Messages.api.APIProcessDataError} {jsonEx.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = Messages.ErrorProcess(ex.Message);
            }

            return Page();
        }
    }
}