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
        
        [BindProperty]
        [Required(ErrorMessage = "Selecione o tipo de importação.")]
        public string TipoImportacao { get; set; }

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
                // Envia em lotes de 1000
                const int loteSize = 1000;

                var produtos = new List<object>();
                var invents  = new List<object>();
                var oxfords  = new List<object>();
                var taxInf   = new List<object>();

                var token = User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value;
                if (string.IsNullOrWhiteSpace(token))
                {
                    ErrorMessage = Messages.import.InvalidSession;
                    return RedirectToPage("/Login");
                }

                while (!reader.EndOfStream)
                {
                    var linha = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(linha)) continue;

                    var campos = linha.Split(';');

                    if (TipoImportacao == "produtos")
                    {
                        // Processa como produtos

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
                    else if (TipoImportacao == "estoque")
                    {
                        // Processa informações de estoque

                        if (campos.Length < 11)
                        {
                            ErrorMessage = Messages.import.InvalidLine11;
                            return Page();
                        }

                        var invent = new
                        {
                            Id = 0,
                            ProductId       = campos[0],
                            NetWeight       = string.IsNullOrWhiteSpace(campos[1]) ? (decimal?)null : decimal.Parse(campos[1].Replace(",", ".")),
                            TaraWeight      = string.IsNullOrWhiteSpace(campos[2]) ? (decimal?)null : decimal.Parse(campos[2].Replace(",", ".")),
                            GrossWeight     = string.IsNullOrWhiteSpace(campos[3]) ? (decimal?)null : decimal.Parse(campos[3].Replace(",", ".")),
                            GrossDepth      = string.IsNullOrWhiteSpace(campos[4]) ? (decimal?)null : decimal.Parse(campos[4].Replace(",", ".")),
                            GrossWidth      = string.IsNullOrWhiteSpace(campos[5]) ? (decimal?)null : decimal.Parse(campos[5].Replace(",", ".")),
                            GrossHeight     = string.IsNullOrWhiteSpace(campos[6]) ? (decimal?)null : decimal.Parse(campos[6].Replace(",", ".")),
                            UnitVolume      = string.IsNullOrWhiteSpace(campos[7]) ? (decimal?)null : decimal.Parse(campos[7].Replace(",", ".")),
                            UnitVolumeML    = string.IsNullOrWhiteSpace(campos[8]) ? (decimal?)null : decimal.Parse(campos[8].Replace(",", ".")),
                            NrOfItems = string.IsNullOrWhiteSpace(campos[9])
                                        ? (int?)null
                                        : (int)decimal.Parse(campos[9].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture),
                        UnitId          = string.IsNullOrWhiteSpace(campos[10]) ? null : campos[10]
                        };

                        invents.Add(invent);
                    }
                    else if (TipoImportacao == "oxford")
                    {
                        // Processa Oxford
                        if (campos.Length < 20)
                        {
                            ErrorMessage = Messages.import.InvalidLine11;
                            return Page();
                        }

                        var oxford = new
                        {
                            Id                      = 0,
                            ProductId               = campos[0],
                            FamilyId                = campos[1],
                            FamilyDescription       = string.IsNullOrWhiteSpace(campos[2]) ? null : campos[2],
                            BrandId                 = campos[3],
                            BrandDescription        = string.IsNullOrWhiteSpace(campos[4]) ? null : campos[4],
                            DecorationId            = campos[5],
                            DecorationDescription   = string.IsNullOrWhiteSpace(campos[6]) ? null : campos[6],
                            TypeId                  = campos[7],
                            TypeDescription         = string.IsNullOrWhiteSpace(campos[8]) ? null : campos[8],
                            ProcessId               = campos[9],
                            ProcessDescription      = string.IsNullOrWhiteSpace(campos[10]) ? null : campos[10],
                            SituationId             = campos[11],
                            SituationDescription    = string.IsNullOrWhiteSpace(campos[12]) ? null : campos[12],
                            LineId                  = campos[13],
                            LineDescription         = string.IsNullOrWhiteSpace(campos[14]) ? null : campos[14],
                            QualityId               = campos[15],
                            QualityDescription      = string.IsNullOrWhiteSpace(campos[16]) ? null : campos[16],
                            BaseProductId           = campos[17],
                            BaseProductDescription  = string.IsNullOrWhiteSpace(campos[18]) ? null : campos[18],
                            ProductGroupId          = campos[19],
                            ProductGroupDescription = string.IsNullOrWhiteSpace(campos[20]) ? null : campos[20]
                        };

                        oxfords.Add(oxford);
                    }
                    else if (TipoImportacao == "fiscal")
                    {
                        // Processa informações fiscais
                        if (campos.Length < 8)
                        {
                            ErrorMessage = Messages.import.InvalidLine8;
                            return Page();
                        }

                        var tax = new
                        {
                            Id = 0,
                            ProductId               = campos[0],
                            TaxationOrigin          = string.IsNullOrWhiteSpace(campos[1]) ? null : campos[1],
                            TaxFiscalClassification = string.IsNullOrWhiteSpace(campos[2]) ? null : campos[2],
                            ProductType             = string.IsNullOrWhiteSpace(campos[3]) ? null : campos[3],
                            CestCode                = string.IsNullOrWhiteSpace(campos[4]) ? null : campos[4],
                            FiscalGroupId           = string.IsNullOrWhiteSpace(campos[5]) ? null : campos[5],
                            ApproxTaxValueFederal   = string.IsNullOrWhiteSpace(campos[6]) ? (decimal?)null : decimal.Parse(campos[6].Replace(",", ".")),
                            ApproxTaxValueState     = string.IsNullOrWhiteSpace(campos[7]) ? (decimal?)null : decimal.Parse(campos[7].Replace(",", ".")),
                            ApproxTaxValueCity      = string.IsNullOrWhiteSpace(campos[8]) ? (decimal?)null : decimal.Parse(campos[8].Replace(",", ".")),
                        };

                        taxInf.Add(tax);
                    }

                }

                if (TipoImportacao == "produtos")
                {
                    int total = produtos.Count;
                    int enviados = 0;

                    for (int i = 0; i < total; i += loteSize)
                    {
                        var lote = produtos.Skip(i).Take(loteSize).ToList();
                        var (success, error) = await _productService.ImportProductAsync(lote, token);

                        if (!success)
                        {
                            ErrorMessage = $"{Messages.import.ErrorImpBatch} {i / loteSize + 1}: {error}";
                            return Page();
                        }

                        enviados += lote.Count;
                    }
                    SuccessMessage = $"{Messages.import.SuccessFull} {enviados}.";

                }
                else if (TipoImportacao == "estoque")
                {
                    // Processa informações de estoque
                    int total = invents.Count;
                    int enviados = 0;

                    for (int i = 0; i < total; i += loteSize)
                    {
                        var lote = invents.Skip(i).Take(loteSize).ToList();
                        var (success, error) = await _productService.ImportInventAsync(lote, token);

                        if (!success)
                        {
                            ErrorMessage = $"{Messages.import.ErrorImpBatch} {i / loteSize + 1}: {error}";
                            return Page();
                        }

                        enviados += lote.Count;
                    }
                    SuccessMessage = $"{Messages.import.SuccessFull} {enviados}.";

                }
                else if (TipoImportacao == "oxford")
                {
                    // Processa Oxford
                    int total = oxfords.Count;
                    int enviados = 0;

                    for (int i = 0; i < total; i += loteSize)
                    {
                        var lote = oxfords.Skip(i).Take(loteSize).ToList();
                        var (success, error) = await _productService.ImportOxfordAsync(lote, token);

                        if (!success)
                        {
                            ErrorMessage = $"{Messages.import.ErrorImpBatch} {i / loteSize + 1}: {error}";
                            return Page();
                        }

                        enviados += lote.Count;
                    }
                    SuccessMessage = $"{Messages.import.SuccessFull} {enviados}.";

                }
                else if (TipoImportacao == "fiscal")
                {
                    // Processa informações fiscais
                    int total = taxInf.Count;
                    int enviados = 0;

                    for (int i = 0; i < total; i += loteSize)
                    {
                        var lote = taxInf.Skip(i).Take(loteSize).ToList();
                        var (success, error) = await _productService.ImportTaxInformationAsync(lote, token);

                        if (!success)
                        {
                            ErrorMessage = $"{Messages.import.ErrorImpBatch} {i / loteSize + 1}: {error}";
                            return Page();
                        }

                        enviados += lote.Count;
                    }
                    SuccessMessage = $"{Messages.import.SuccessFull} {enviados}.";

                }
            }
            catch (Exception ex)
            {
                ErrorMessage = Messages.ErrorProcess(ex.Message);
            }

            return Page();
        }
    }
}