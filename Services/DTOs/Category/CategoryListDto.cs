namespace HadiyahServices.DTOs.Category
{
    public class CategoryListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; }
    }
}
