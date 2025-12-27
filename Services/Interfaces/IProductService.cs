using HadiyahServices.DTOs.Common;
using HadiyahServices.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.Interfaces
{
    public interface IProductService
    {
        Task<BaseResponse<ProductListDto>> CreateAsync(ProductCreateDto dto);
        Task<BaseResponse<ProductListDto>> UpdateAsync(ProductUpdateDto dto);
        Task<BaseResponse<bool>> DeleteAsync(long id);
        Task<BaseResponse<IEnumerable<ProductListDto>>> GetAllAsync();
        Task<BaseResponse<IEnumerable<ProductListDto>>> GetFilteredAsync(long? categoryId, decimal? minPrice, decimal? maxPrice, int? skip = null, int? take = null);
        Task<BaseResponse<ProductPagedResultDto>> GetFilteredPagedAsync(long? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12);
        Task<BaseResponse<ProductListDto>> GetByIdAsync(long id);
        Task<BaseResponse<List<ProductListDto>>> GetByCategoryAsync(long categoryId);
    }
}
