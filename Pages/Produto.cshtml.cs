using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using OXF.Constants;
using OxfordOnline.Dtos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace OXF.Pages;

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
    public List<string> Imagens { get; set; } = new();
    public string ErrorMessage { get; set; }

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

            var imageUrl = $"{baseUrl}/v1/Image/Product/{produtoId}";
            var imageResponse = await client.GetAsync(imageUrl);
            if (imageResponse.IsSuccessStatusCode)
            {
                var imagensDto = await imageResponse.Content.ReadFromJsonAsync<List<ImageDto>>();
                var cacheFolder = Path.Combine(_env.WebRootPath, "images", "cache");
                Directory.CreateDirectory(cacheFolder);

                foreach (var img in imagensDto.Where(i => !string.IsNullOrWhiteSpace(i.ImagePath)))
                {
                    var ftpRelativePath = img.ImagePath.TrimStart('/').Replace('\\', '/');
                    var fileName = Path.GetFileName(ftpRelativePath);
                    var localFilePath = Path.Combine(cacheFolder, fileName);

                    if (!System.IO.File.Exists(localFilePath))
                    {
                        try
                        {
                            await DownloadFileFromFtpAsync(ftpRelativePath, localFilePath);
                        }
                        catch
                        {
                            // erro ao baixar imagem, ignora e continua
                        }
                    }

                    Imagens.Add($"/images/cache/{fileName}");
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao carregar produto: {ex.Message}";
            Produto = null;
        }

        return Page();
    }

    private async Task DownloadFileFromFtpAsync(string ftpFilePath, string localFilePath)
    {
        var ftpHost = "ftp.oxfordtec.com.br";
        var ftpUser = "u700242432.oxfordftp";
        var ftpPass = "OxforEstrutur@25";

        var ftpUri = new Uri($"ftp://{ftpHost}/{ftpFilePath}");

        var request = (FtpWebRequest)WebRequest.Create(ftpUri);
        request.Method = WebRequestMethods.Ftp.DownloadFile;
        request.Credentials = new NetworkCredential(ftpUser, ftpPass);
        request.UseBinary = true;
        request.UsePassive = true;
        request.KeepAlive = false;

        using var response = (FtpWebResponse)await request.GetResponseAsync();
        using var responseStream = response.GetResponseStream();
        using var fileStream = System.IO.File.Create(localFilePath);
        await responseStream.CopyToAsync(fileStream);
    }

    public class ProdutoResponse
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string Barcode { get; set; }
    }
}
