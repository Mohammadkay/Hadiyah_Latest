using HadiyahServices.DTOs.Category;
using HadiyahServices.DTOs.Common;

namespace HadiyahServices.Interfaces
{
    public interface ICategoryService
    {
        Task<BaseResponse<CategoryListDto>> CreateAsync(CategoryCreateDto dto);
        Task<BaseResponse<CategoryListDto>> UpdateAsync(CategoryUpdateDto dto);
        Task<BaseResponse<bool>> DeleteAsync(int id);
        Task<BaseResponse<IEnumerable<CategoryListDto>>> GetAllAsync();
        Task<BaseResponse<CategoryListDto>> GetByIdAsync(int id);
    }
}
