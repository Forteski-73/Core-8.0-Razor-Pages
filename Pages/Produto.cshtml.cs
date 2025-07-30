using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using OXF.Constants;
using OXF.Models;
using OxfordOnline.Dtos;
using OxfordOnline.Models.Enums;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace OXF.Pages;

//[IgnoreAntiforgeryToken]
public class ProdutoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;
    private readonly string _baseUrl;

    public ProdutoModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, IWebHostEnvironment env)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _env = env;
        _baseUrl = _configuration["ApiSettings:BaseUrl"];
    }

    public ProdutoResponse Produto { get; set; }
    public List<string> ImgProduto { get; set; } = new();

    public List<string> ImgPackage { get; set; } = new();

    public List<string> ImgPallet { get; set; } = new();

    public Invent? Invent { get; set; }

    public TaxInformation? TaxInformation { get; set; }

    public Oxford? Oxford { get; set; }

    public string ErrorMessage { get; set; }

    public string SuccessMessage { get; set; }

    [BindProperty]
    public List<IFormFile> NovasImagens { get; set; } = new();

    public string Id { get; set; }

    public class TesteRequest
    {
        public string Id { get; set; }
    }


    public async Task<IActionResult> OnPostUploadBase64Async(string product, Finalidade finalidade)
    {
        if (string.IsNullOrWhiteSpace(product) || !Request.Form.Files.Any())
            return BadRequest("Nenhuma imagem enviada.");

        try
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized();

            var client = CreateHttpClientWithToken(token);

            using var content = new MultipartFormDataContent();

            foreach (var file in Request.Form.Files)
            {
                var stream = file.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "files", file.FileName);
            }

            var response = await client.PostAsync($"{_baseUrl}/v1/Image/ReplaceProductImages/{product}/{finalidade}", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, $"Erro ao salvar imagens: {response.ReasonPhrase}");

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao salvar imagens: {ex.Message}");
        }
    }

    private HttpClient CreateHttpClientWithToken(string token)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Page();

        try
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                ErrorMessage = Messages.import.InvalidSession;
                return RedirectToPage("/Login");
            }

            var client = CreateHttpClientWithToken(token);

            var produtoId = id.PadLeft(5, '0');
            var url = $"{_baseUrl}/v1/Product/{produtoId}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Produto = null;
                return Page();
            }

            Produto = await response.Content.ReadFromJsonAsync<ProdutoResponse>();


            // Carregar as três finalidades
            ImgProduto      = await LoadImagesFromZipAsync(client, produtoId, Finalidade.PRODUTO);
            ImgPackage      = await LoadImagesFromZipAsync(client, produtoId, Finalidade.EMBALAGEM);
            ImgPallet       = await LoadImagesFromZipAsync(client, produtoId, Finalidade.PALETIZACAO);

            Invent = await GetInventFromApiAsync(client, produtoId);

            TaxInformation = await GetTaxInformationFromApiAsync(client, produtoId);

            Oxford = await GetOxfordFromApiAsync(client, produtoId);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar produto: {ex.Message}";
            Produto = null;
        }

        return Page();
    }

    private async Task<Invent?> GetInventFromApiAsync(HttpClient client, string productId)
    {
        var inventUrl = $"{_baseUrl}/v1/Invent/{productId}";

        var response = await client.GetAsync(inventUrl);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var invent = JsonSerializer.Deserialize<Invent>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return invent;
    }

    private async Task<TaxInformation?> GetTaxInformationFromApiAsync(HttpClient client, string productId)
    {
        var inventUrl = $"{_baseUrl}/v1/TaxInformation/{productId}";

        var response = await client.GetAsync(inventUrl);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var taxInformation = JsonSerializer.Deserialize<TaxInformation>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return taxInformation;
    }

    private async Task<Oxford?> GetOxfordFromApiAsync(HttpClient client, string productId)
    {
        var inventUrl = $"{_baseUrl}/v1/Oxford/{productId}";

        var response = await client.GetAsync(inventUrl);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var oxford = JsonSerializer.Deserialize<Oxford>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return oxford;
    }

    private async Task<List<string>> LoadImagesFromZipAsync(HttpClient client, string produtoId, Finalidade finalidade)
    {
        var images = new List<string>();
        var zipUrl = $"{_baseUrl}/v1/Image/ProductImage/{produtoId}/{finalidade}/false";
        var zipResponse = await client.GetAsync(zipUrl);

        if (zipResponse.IsSuccessStatusCode)
        {
            await using var zipStream = await zipResponse.Content.ReadAsStreamAsync();
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Name)) continue;

                using var entryStream = entry.Open();
                using var ms = new MemoryStream();
                await entryStream.CopyToAsync(ms);
                var bytes = ms.ToArray();

                var contentType = GetContentType(entry.Name);
                var base64 = Convert.ToBase64String(bytes);
                var dataUri = $"data:{contentType};base64,{base64}";

                images.Add(dataUri);
            }
        }

        return images;
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


    public class ProdutoResponse
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
    }
}