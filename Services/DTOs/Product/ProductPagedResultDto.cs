using System.Collections.Generic;

namespace HadiyahServices.DTOs.Product
{
    public class ProductPagedResultDto
    {
        public IEnumerable<ProductListDto> Items { get; set; } = new List<ProductListDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
