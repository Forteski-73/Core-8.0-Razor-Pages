using System.ComponentModel.DataAnnotations;

namespace OXF.Models
{
    public class Invent
    {
        public int Id { get; set; }

        [Display(Name = "ID do Produto")]
        public string ProductId { get; set; } = string.Empty;

        [Display(Name = "Peso Líquido")]
        public decimal? NetWeight { get; set; }

        [Display(Name = "Peso Tara")]
        public decimal? TaraWeight { get; set; }

        [Display(Name = "Peso Bruto")]
        public decimal? GrossWeight { get; set; }

        [Display(Name = "Profundidade Bruta")]
        public decimal? GrossDepth { get; set; }

        [Display(Name = "Largura Bruta")]
        public decimal? GrossWidth { get; set; }

        [Display(Name = "Altura Bruta")]
        public decimal? GrossHeight { get; set; }

        [Display(Name = "Volume Unitário")]
        public decimal? UnitVolume { get; set; }

        [Display(Name = "Volume Unitário (mL)")]
        public decimal? UnitVolumeML { get; set; }

        [Display(Name = "Número de Itens")]
        public int? NrOfItems { get; set; }

        [Display(Name = "ID da Unidade")]
        public string? UnitId { get; set; }
    }
}
