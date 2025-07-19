using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using OXF.Constants;
using OxfordOnline.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO.Compression;
using OxfordOnline.Models.Enums;

namespace OXF.Pages;

//[IgnoreAntiforgeryToken]
public class ProdutoModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public ProdutoModel(IHttpClientFactory httpClientFactory, IConfiguration configuration, IWebHostEnvironment env)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _env = env;
    }

    public ProdutoResponse Produto { get; set; }
    public List<string> ImgDecoration { get; set; } = new();

    public List<string> ImgPackage { get; set; } = new();

    public List<string> ImgPallet { get; set; } = new();

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

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();

            foreach (var file in Request.Form.Files)
            {
                var stream = file.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "files", file.FileName);
            }

            var response = await client.PostAsync($"{baseUrl}/v1/Image/ReplaceProductImages/{product}", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, $"Erro ao salvar imagens: {response.ReasonPhrase}");

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao salvar imagens: {ex.Message}");
        }
    }

    /*
    public async Task<IActionResult> OnPostUploadBase64Async([FromQuery] string product)
    {
        if (string.IsNullOrWhiteSpace(product) || !Request.Form.Files.Any())
            return BadRequest("Nenhuma imagem enviada.");

        try
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized();

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();

            foreach (var file in Request.Form.Files)
            {
                var stream = file.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "files", file.FileName);
            }

            var response = await client.PostAsync($"{baseUrl}/v1/Image/ReplaceProductImages/{product}", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, $"Erro ao salvar imagens: {response.ReasonPhrase}");

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao salvar imagens: {ex.Message}");
        }
    }
    */

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

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var produtoId = id.PadLeft(5, '0');
            var url = $"{baseUrl}/v1/Product/{produtoId}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Produto = null;
                return Page();
            }

            Produto = await response.Content.ReadFromJsonAsync<ProdutoResponse>();


            // Carregar as três finalidades
            ImgDecoration   = await LoadImagesFromZipAsync(client, baseUrl, produtoId, Finalidade.DECORACAO);
            ImgPackage      = await LoadImagesFromZipAsync(client, baseUrl, produtoId, Finalidade.EMBALAGEM);
            ImgPallet       = await LoadImagesFromZipAsync(client, baseUrl, produtoId, Finalidade.PALETIZACAO);
        

            /*
            // Novo método com .zip das imagens do produto
            var zipUrl = $"{baseUrl}/v1/Image/ProductImage/{produtoId}/{Finalidade.DECORACAO}";
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

                    ImgDecoration.Add(dataUri);
                }
            }
            */
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar produto: {ex.Message}";
            Produto = null;
        }

        return Page();
    }

    private async Task<List<string>> LoadImagesFromZipAsync(HttpClient client, string baseUrl, string produtoId, Finalidade finalidade)
    {
        var images = new List<string>();
        var zipUrl = $"{baseUrl}/v1/Image/ProductImage/{produtoId}/{finalidade}";
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


    /*
    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || NovasImagens == null || !NovasImagens.Any())
            return Page();

        try
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                ErrorMessage = Messages.import.InvalidSession;
                return RedirectToPage("/Login");
            }

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();

            foreach (var imagem in NovasImagens)
            {
                var stream = imagem.OpenReadStream();
                var streamContent = new StreamContent(stream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(imagem.ContentType);
                content.Add(streamContent, "files", imagem.FileName);
            }

            var response = await client.PostAsync($"{baseUrl}/v1/Image/ReplaceProductImages/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Erro ao enviar imagens: {response.ReasonPhrase}";
            }
            else
            {
                SuccessMessage = "Imagens atualizadas com sucesso!";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao enviar imagens: {ex.Message}";
        }

        return RedirectToPage(new { id });
    }*/


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