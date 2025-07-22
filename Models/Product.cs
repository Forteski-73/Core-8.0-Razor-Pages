using System.ComponentModel.DataAnnotations;

namespace OXF.Models
{
    public class Product
    {
        [Display(Name = "Produto")]
        public string ProductId { get; set; } = string.Empty;
        [Display(Name = "Descrição")]
        public string ProductName { get; set; } = string.Empty;
        [Display(Name = "Cód. de Barras")]
        public string Barcode { get; set; } = string.Empty;
    }
}
