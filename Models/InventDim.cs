using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OXF.Models
{
    public class InventDim
    {
        public int Id { get; set; }

        [Display(Name = "Produto")]
        public string ProductId { get; set; } = string.Empty;
        [Display(Name = "Localização")]
        public string? LocationId { get; set; }
        [Display(Name = "Empresa")]
        public string? CompanyId { get; set; }
        [Display(Name = "Quantidade")]
        public int? Quantity { get; set; }
        [Display(Name = "Preço")]
        public decimal? Price { get; set; }
    }
}
