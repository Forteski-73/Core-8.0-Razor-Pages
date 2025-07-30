using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Constants;
using OXF.Services;

namespace OXF.Pages;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class SincronizarModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<IndexModel> _logger;

    public SincronizarModel(IProductService productService, ILogger<IndexModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public async Task<IActionResult> OnPostAsync(string syncOption, string? productId)
    {
        var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
        if (string.IsNullOrWhiteSpace(token))
        {
            return RedirectToPage("/Login");
        }
        else
        {
            var produtos = new[]
            {
                new { } // Produto em branco
            };
            if (syncOption == "byProduct")
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    ModelState.AddModelError("", "Informe o código do produto.");
                    return Page();
                }

                // Sincronizar produto específico
                await _productService.SyncProductAsync(produtos, token);
            }
            else if (syncOption == "all")
            {
                // Sincronizar todos
                await _productService.SyncProductAsync(produtos, token);
            }
        }


        return RedirectToPage();
    }
}
