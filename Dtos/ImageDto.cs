namespace OxfordOnline.Dtos
{
    public class ImageDto
    {
        public int? Id { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public int? Sequence { get; set; }
        public bool? ImageMain { get; set; }
        public string? Finalidade { get; set; }

    }
}
