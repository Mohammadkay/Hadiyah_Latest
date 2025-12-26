using System;
using System.IO;
using System.Linq;
using HadiyahDomain.Entities;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Category;
using HadiyahServices.DTOs.Common;
using HadiyahServices.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HadiyahServices.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly string _imageRoot;

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
            _imageRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        }

        public async Task<BaseResponse<CategoryListDto>> CreateAsync(CategoryCreateDto dto)
        {
            if (await _categoryRepo.ExistsByNameAsync(dto.Name))
                return BaseResponse<CategoryListDto>.Fail("Category name already exists");

            var imagePath = await SaveImageToFileAsync(dto.ImageBase64);

            var entity = new Category
            {
                Name = dto.Name,
                ImagePath = imagePath
            };

            await _categoryRepo.AddAsync(entity);

            return BaseResponse<CategoryListDto>.Success(new CategoryListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ImagePath = entity.ImagePath,
                IsActive = entity.IsActive
            });
        }

        public async Task<BaseResponse<CategoryListDto>> UpdateAsync(CategoryUpdateDto dto)
        {
            var category = await _categoryRepo.GetByIdAsync(dto.Id);
            if (category == null)
                return BaseResponse<CategoryListDto>.Fail("Category not found");

            category.Name = dto.Name;
            category.IsActive = dto.IsActive;

            if (!string.IsNullOrWhiteSpace(dto.ImageBase64))
            {
                var newPath = await SaveImageToFileAsync(dto.ImageBase64);
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    DeleteImageFile(category.ImagePath);
                    category.ImagePath = newPath;
                }
            }

            await _categoryRepo.UpdateAsync(category);

            return BaseResponse<CategoryListDto>.Success(new CategoryListDto
            {
                Id = category.Id,
                Name = category.Name,
                ImagePath = category.ImagePath,
                IsActive = category.IsActive
            });
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null)
                return BaseResponse<bool>.Fail("Category not found");

            DeleteImageFile(category.ImagePath);
            await _categoryRepo.DeleteAsync(category);
            return BaseResponse<bool>.Success(true);
        }

        public async Task<BaseResponse<IEnumerable<CategoryListDto>>> GetAllAsync()
        {
            var categories = _categoryRepo.GetAllAsQurable();

            var list = await categories.Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                ImagePath = c.ImagePath,
                IsActive = c.IsActive
            }).ToListAsync();

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
                ImagePath = category.ImagePath,
                IsActive = category.IsActive
            });
        }

        private async Task<string?> SaveImageToFileAsync(string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return null;

            var clean = base64;
            var extension = ".png";

            if (base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var commaIndex = base64.IndexOf(',');
                if (commaIndex > -1)
                {
                    var meta = base64.Substring(5, commaIndex - 5);
                    if (meta.Contains("jpeg", StringComparison.OrdinalIgnoreCase))
                        extension = ".jpg";
                    else if (meta.Contains("png", StringComparison.OrdinalIgnoreCase))
                        extension = ".png";

                    clean = base64[(commaIndex + 1)..];
                }
            }

            try
            {
                var bytes = Convert.FromBase64String(clean);
                Directory.CreateDirectory(_imageRoot);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(_imageRoot, fileName);
                await File.WriteAllBytesAsync(filePath, bytes);
                return $"/images/{fileName}";
            }
            catch
            {
                return null;
            }
        }

        private void DeleteImageFile(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return;

            try
            {
                var trimmed = imagePath.TrimStart('~').TrimStart('/');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), trimmed.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch
            {
                // ignore cleanup errors
            }
        }
    }
}
