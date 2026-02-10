using System.ComponentModel.DataAnnotations;

namespace OXF.Models
{
    public class Inventory
    {
        [Key]
        [Display(Name = "Código")]
        public string InventCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Inventário")]
        public string? InventName { get; set; }

        [Display(Name = "GUID")]
        public string? InventGuid { get; set; }

        [Display(Name = "Setor")]
        public string? InventSector { get; set; }

        [Display(Name = "Data")]
        public DateTime? InventCreated { get; set; }

        [Display(Name = "Usuário")]
        public string? InventUser { get; set; }

        [Display(Name = "Status")]
        public string InventStatus { get; set; } = "Iniciado";

        [Display(Name = "Total")]
        public decimal InventTotal { get; set; }

        // Propriedade de Navegação para as linhas
        public List<InventoryRecord> Records { get; set; } = new();
    }
}