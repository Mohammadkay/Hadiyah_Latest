using Domain.Entities;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Category;
using HadiyahServices.DTOs.Common;
using HadiyahServices.Interfaces;

namespace HadiyahServices.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<BaseResponse<CategoryListDto>> CreateAsync(CategoryCreateDto dto)
        {
            if (await _categoryRepo.ExistsByNameAsync(dto.Name))
                return BaseResponse<CategoryListDto>.Fail("Category name already exists");

            var entity = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                ImageBase64 = dto.ImageBase64
            };

            await _categoryRepo.AddAsync(entity);

            return BaseResponse<CategoryListDto>.Success(new CategoryListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                ImageBase64 = entity.ImageBase64,
                IsActive = entity.IsActive
            });
        }

        public async Task<BaseResponse<CategoryListDto>> UpdateAsync(CategoryUpdateDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(dto.Id);
            if (category == null)
                return BaseResponse<CategoryListDto>.Fail("Category not found");

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.ImageBase64 = dto.ImageBase64;
            category.IsActive = dto.IsActive;

            await _categoryRepo.UpdateAsync(category);

            return BaseResponse<CategoryListDto>.Success(new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageBase64 = category.ImageBase64,
                IsActive = category.IsActive
            });
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
                return BaseResponse<bool>.Fail("Category not found");

            await _categoryRepo.DeleteAsync(category);
            return BaseResponse<bool>.Success(true);
        }

        public async Task<BaseResponse<IEnumerable<CategoryListDto>>> GetAllAsync()
        {
            var categories = await _categoryRepo.GetAllAsync();

            var list = categories.Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ImageBase64 = c.ImageBase64,
                IsActive = c.IsActive
            });

            return BaseResponse<IEnumerable<CategoryListDto>>.Success(list);
        }

        public async Task<BaseResponse<CategoryListDto>> GetByIdAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);

            if (category == null)
                return BaseResponse<CategoryListDto>.Fail("Category not found");

            return BaseResponse<CategoryListDto>.Success(new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageBase64 = category.ImageBase64,
                IsActive = category.IsActive
            });
        }
    }
}
