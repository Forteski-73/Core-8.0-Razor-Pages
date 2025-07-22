using System.ComponentModel.DataAnnotations;

namespace OXF.Models
{
    public class TaxInformation
    {
        public int Id { get; set; }

        [Display(Name = "ID do Produto")]
        public string ProductId { get; set; } = string.Empty;

        // Tributação
        [Display(Name = "Origem da Tributação")]
        public string? TaxationOrigin { get; set; }

        [Display(Name = "Classificação Fiscal")]
        public string? TaxFiscalClassification { get; set; }

        [Display(Name = "Tipo de Produto")]
        public string? ProductType { get; set; }

        [Display(Name = "Código CEST")]
        public string? CestCode { get; set; }

        [Display(Name = "ID do Grupo Fiscal")]
        public string? FiscalGroupId { get; set; }

        // Percentuais aproximados de imposto
        [Display(Name = "Valor Aproximado Imposto Federal (%)")]
        public decimal? ApproxTaxValueFederal { get; set; }

        [Display(Name = "Valor Aproximado Imposto Estadual (%)")]
        public decimal? ApproxTaxValueState { get; set; }

        [Display(Name = "Valor Aproximado Imposto Municipal (%)")]
        public decimal? ApproxTaxValueCity { get; set; }
    }
}
