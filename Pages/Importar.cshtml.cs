using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OXF.Constants;
using OXF.Services;
using System.ComponentModel.DataAnnotations;

namespace OXF.Pages
{
    public class ImportarModel : PageModel
    {
        private readonly IProductService _productService;

        public ImportarModel(IProductService productService)
        {
            _productService = productService;
        }

        [BindProperty]
        [Required(ErrorMessage = Messages.import.FileRequired)]
        public IFormFile ArquivoImportacao { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = Messages.import.FileRequired;
                return Page();
            }

            var extensao = Path.GetExtension(ArquivoImportacao.FileName).ToLowerInvariant();
            if (extensao != ".txt" && extensao != ".csv")
            {
                ErrorMessage = Messages.import.InvalidLFormat;
                return Page();
            }

            try
            {
                using var stream = ArquivoImportacao.OpenReadStream();
                using var reader = new StreamReader(stream);
                var produtos = new List<object>();

                while (!reader.EndOfStream)
                {
                    var linha = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(linha)) continue;

                    var campos = linha.Split(';');
                    if (campos.Length < 3)
                    {
                        ErrorMessage = Messages.import.InvalidLine;
                        return Page();
                    }

                    var produto = new
                    {
                        id = 0,
                        productId = campos[0],
                        productName = campos[1],
                        barcode = campos[2],
                        // outros campos...
                    };

                    produtos.Add(produto);
                }

                var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage = Messages.import.InvalidSession;
                    return RedirectToPage("/Login");
                }

                // Envia em lotes de 1000
                const int loteSize = 1000;
                int total = produtos.Count;
                int enviados = 0;

                for (int i = 0; i < total; i += loteSize)
                {
                    var lote = produtos.Skip(i).Take(loteSize).ToList();
                    var (success, error) = await _productService.ImportAsync(lote, token);

                    if (!success)
                    {
                        ErrorMessage = $"{Messages.import.ErrorImpBatch} {i / loteSize + 1}: {error}";
                        return Page();
                    }

                    enviados += lote.Count;
                }

                SuccessMessage = $"{Messages.import.SuccessFull} {enviados}.";
            }
            catch (Exception ex)
            {
                ErrorMessage = Messages.ErrorProcess(ex.Message);
            }

            return Page();
        }
    }
}