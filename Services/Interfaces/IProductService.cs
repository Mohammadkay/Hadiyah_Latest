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
        Task<BaseResponse<ProductListDto>> GetByIdAsync(long id);
        Task<BaseResponse<List<ProductListDto>>> GetByCategoryAsync(long categoryId);
    }
}
