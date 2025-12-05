namespace HadiyahServices.DTOs.Category
{
    public class CategoryListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? ImageBase64 { get; set; }
        public bool IsActive { get; set; }
    }
}
