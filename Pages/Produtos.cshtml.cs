using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Constants;
using OXF.Models;
using OxfordOnline.Models.Enums;
using System.IO.Compression;
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
        private readonly string _baseUrl;

        public ProductModel(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _baseUrl = _configuration["ApiSettings:BaseUrl"];
        }

        public List<Product> Product { get; set; } = new();
        public List<string> ImgProduto { get; set; } = new();


        [BindProperty(SupportsGet = true)]
        public string? Produto { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CodigoDeBarras { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Familia { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Marca { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Linha { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Decoracao { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Nome { get; set; }


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
                var url = $"{baseUrl}{ApiEndpoints.Product.Search}";

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

                // Carregar imagem principal para cada produto
                /*foreach (var produto in Product)
                {
                    var imagem = await LoadMainImageFromZipAsync(client, produto.ProductId, Finalidade.PRODUTO);
                    produto.ImageBase64 = imagem; // pode ser null se não houver imagem
                }*/

                // Carregar imagens principais de forma paralela
                // Task.WhenAll para paralelizar a busca de imagens
                var tasks = Product.Select(async produto =>
                {
                    produto.ImageBase64 = await LoadMainImageFromZipAsync(client, produto.ProductId, Finalidade.PRODUTO);
                });

                await Task.WhenAll(tasks);
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

        private async Task<string?> LoadMainImageFromZipAsync(HttpClient client, string produtoId, Finalidade finalidade)
        {
            var zipUrl = $"{_baseUrl}/v1/Image/ProductImage/{produtoId}/{finalidade}/true"; // <-- main = true
            var zipResponse = await client.GetAsync(zipUrl);

            if (zipResponse.IsSuccessStatusCode)
            {
                await using var zipStream = await zipResponse.Content.ReadAsStreamAsync();
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

                var entry = archive.Entries.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Name));
                if (entry != null)
                {
                    using var entryStream = entry.Open();
                    using var ms = new MemoryStream();
                    await entryStream.CopyToAsync(ms);
                    var bytes = ms.ToArray();

                    var contentType = GetContentType(entry.Name);
                    var base64 = Convert.ToBase64String(bytes);
                    return $"data:{contentType};base64,{base64}";
                }
            }

            return null;
        }
        private static string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        public async Task<IActionResult> OnGetSearchAsync(string searchTerm, string searchField)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return RedirectToPage("/Login");

            try
            {
                var query = Request.Query;

                string? codigoDeBarras = query.TryGetValue("Código de Barras", out var cb) ? cb.ToString() : null;
                string? produto = query.TryGetValue("Produto", out var p) ? p.ToString() : null;
                string? familia = query.TryGetValue("Família", out var f) ? f.ToString() : null;
                string? marca = query.TryGetValue("Marca", out var m) ? m.ToString() : null;
                string? linha = query.TryGetValue("Linha", out var l) ? l.ToString() : null;
                string? decoracao = query.TryGetValue("Decoração", out var d) ? d.ToString() : null;
                string? nome = query.TryGetValue("Nome", out var n) ? n.ToString() : null;

                var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                    return RedirectToPage("/Login");

                var queryString = new Dictionary<string, string?>
                {
                    ["product"] = produto,
                    ["barcode"] = codigoDeBarras,
                    ["family"] = familia,
                    ["brand"] = marca,
                    ["line"] = linha,
                    ["decoration"] = decoracao,
                    ["nome"] = nome
                };

                // Monta a query string apenas com valores não nulos/vazios
                var queryParams = string.Join("&",
                    queryString
                        .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                        .Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value!)}"));

                var url = $"{_baseUrl}/v1/Product/Search";
                if (!string.IsNullOrWhiteSpace(queryParams))
                    url += "?" + queryParams;

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = $"Erro ao buscar produtos: {response.ReasonPhrase}";
                    return Page();
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                Product = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new();

                // Carrega imagens principais
                var tasks = Product.Select(async produto =>
                {
                    produto.ImageBase64 = await LoadMainImageFromZipAsync(client, produto.ProductId, Finalidade.PRODUTO);
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                ErrorMessage = Messages.ErrorProcess(ex.Message);
            }

            return Partial("_ProductListPartial", Product);
        }
    }
}