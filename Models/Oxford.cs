using System.ComponentModel.DataAnnotations;

namespace OXF.Models
{
    public class Oxford
    {
        public int Id { get; set; }

        [Display(Name = "ID do Produto")]
        public string ProductId { get; set; } = string.Empty;

        [Display(Name = "ID da Família")]
        public string FamilyId { get; set; } = string.Empty;

        [Display(Name = "Família")]
        public string? FamilyDescription { get; set; }

        [Display(Name = "ID da Marca")]
        public string BrandId { get; set; } = string.Empty;

        [Display(Name = "Marca")]
        public string? BrandDescription { get; set; }

        [Display(Name = "ID da Decoração")]
        public string DecorationId { get; set; } = string.Empty;

        [Display(Name = "Decoração")]
        public string? DecorationDescription { get; set; }

        [Display(Name = "ID do Tipo")]
        public string TypeId { get; set; } = string.Empty;

        [Display(Name = "Tipo")]
        public string? TypeDescription { get; set; }

        [Display(Name = "ID do Processo")]
        public string ProcessId { get; set; } = string.Empty;

        [Display(Name = "Processo")]
        public string? ProcessDescription { get; set; }

        [Display(Name = "ID da Situação")]
        public string SituationId { get; set; } = string.Empty;

        [Display(Name = "Situação")]
        public string? SituationDescription { get; set; }

        [Display(Name = "ID da Linha")]
        public string LineId { get; set; } = string.Empty;

        [Display(Name = "Linha")]
        public string? LineDescription { get; set; }

        [Display(Name = "ID da Qualidade")]
        public string QualityId { get; set; } = string.Empty;

        [Display(Name = "Qualidade")]
        public string? QualityDescription { get; set; }

        [Display(Name = "ID do Produto Base")]
        public string BaseProductId { get; set; } = string.Empty;

        [Display(Name = "Produto Base")]
        public string? BaseProductDescription { get; set; }

        [Display(Name = "ID do Grupo de Produto")]
        public string ProductGroupId { get; set; } = string.Empty;

        [Display(Name = "Grupo de Produto")]
        public string? ProductGroupDescription { get; set; }
    }
}
