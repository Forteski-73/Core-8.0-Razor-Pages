using System.ComponentModel.DataAnnotations;

namespace OXF.Models
{
    public class InventoryRecord
    {
        [Key]
        public int? Id { get; set; }

        [Display(Name = "Código Inventário")]
        public string InventCode { get; set; } = string.Empty;

        [Display(Name = "Data Registro")]
        public DateTime? InventCreated { get; set; }

        [Display(Name = "Conferente")]
        public string? InventUser { get; set; }

        [Display(Name = "Unitizador/Palete")]
        public string? InventUnitizer { get; set; }

        [Display(Name = "Endereço/Local")]
        public string? InventLocation { get; set; }

        [Display(Name = "Cód. Produto")]
        public string InventProduct { get; set; } = string.Empty;

        [Display(Name = "Cód. Barras")]
        public string? InventBarcode { get; set; }

        [Display(Name = "Descrição do Produto")]
        public string? ProductDescription { get; set; }

        [Display(Name = "Lastro (Padrão)")]
        public int? InventStandardStack { get; set; }

        [Display(Name = "Qtd. Pilhas (Alt)")]
        public int? InventQtdStack { get; set; }

        [Display(Name = "Qtd. Avulsa")]
        public decimal? InventQtdIndividual { get; set; }

        [Display(Name = "Total Item")]
        public decimal? InventTotal { get; set; }

        // Propriedade auxiliar para facilitar a exibição do cálculo na tela
        [Display(Name = "Cálculo")]
        public string CalculoExtenso => $"({InventQtdStack} x {InventStandardStack}) + {InventQtdIndividual}";
    }
}