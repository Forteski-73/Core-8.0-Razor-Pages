using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Services;
using OXF.Models;

namespace OXF.Pages;

public class InventarioModel : PageModel
{
    private readonly IInventoryService _inventoryService;

    public InventarioModel(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public List<Inventory> Inventarios { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;

            if (string.IsNullOrEmpty(token))
                return RedirectToPage("/Login");

            Inventarios = await _inventoryService.GetAllAsync(token);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }
    public async Task<IActionResult> OnGetDownloadAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return BadRequest("GUID inválido.");

        var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
        if (string.IsNullOrEmpty(token))
            return Unauthorized();

        var records = await _inventoryService.GetRecordsByGuidAsync(code, token)
                      ?? new List<InventoryRecord>();

        foreach (var r in records)
        {
            if (!string.IsNullOrEmpty(r.InventBarcode))
            {
                r.InventBarcode = r.InventBarcode.TrimStart('0');
            }
        }

        return new JsonResult(records);
    }

}
