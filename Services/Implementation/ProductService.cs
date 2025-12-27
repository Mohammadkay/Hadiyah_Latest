using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using HadiyahDomain.Entities;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Common;
using HadiyahServices.DTOs.Product;
using HadiyahServices.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HadiyahServices.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly string _imageRoot;

        public ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _imageRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        }

        private static ProductListDto ToListDto(Product p) => new ProductListDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            ImagePath = p.ImagePath,
            StockQuantity = p.StockQuantity,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name
        };

        public async Task<BaseResponse<ProductListDto>> CreateAsync(ProductCreateDto dto)
        {
            if (await _productRepo.ExistsByNameAsync(dto.Name))
                return BaseResponse<ProductListDto>.Fail("Product name already exists");

            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return BaseResponse<ProductListDto>.Fail("Invalid category");

            var imagePath = await SaveImageToFileAsync(dto.ImageBase64);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImagePath = imagePath,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId
            };

            await _productRepo.AddAsync(product);

            return BaseResponse<ProductListDto>.Success(new ProductListDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImagePath = product.ImagePath,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = category.Name
            });
        }

        public async Task<BaseResponse<ProductListDto>> UpdateAsync(ProductUpdateDto dto)
        {
            var product = await _productRepo.GetByIdAsync(dto.Id);
            if (product == null)
                return BaseResponse<ProductListDto>.Fail("Product not found");

            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return BaseResponse<ProductListDto>.Fail("Invalid category");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;
            product.CategoryId = dto.CategoryId;

            if (!string.IsNullOrWhiteSpace(dto.ImageBase64))
            {
                var newPath = await SaveImageToFileAsync(dto.ImageBase64);
                if (!string.IsNullOrWhiteSpace(newPath))
                {
                    DeleteImageFile(product.ImagePath);
                    product.ImagePath = newPath;
                }
            }

            await _productRepo.UpdateAsync(product);

            return BaseResponse<ProductListDto>.Success(new ProductListDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImagePath = product.ImagePath,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = category.Name
            });
        }

        public async Task<BaseResponse<bool>> DeleteAsync(long id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                return BaseResponse<bool>.Fail("Product not found");

            DeleteImageFile(product.ImagePath);
            await _productRepo.DeleteAsync(product);
            return BaseResponse<bool>.Success(true);
        }

        public async Task<BaseResponse<IEnumerable<ProductListDto>>> GetAllAsync()
        {
            var products = await _productRepo.GetFilteredAsync(null, null, null);
            var result = products.Select(ToListDto);

            return BaseResponse<IEnumerable<ProductListDto>>.Success(result);
        }

        public async Task<BaseResponse<IEnumerable<ProductListDto>>> GetFilteredAsync(long? categoryId, decimal? minPrice, decimal? maxPrice, int? skip = null, int? take = null)
        {
            var products = await _productRepo.GetFilteredAsync(categoryId, minPrice, maxPrice, skip, take);
            var result = products.Select(ToListDto);

            return BaseResponse<IEnumerable<ProductListDto>>.Success(result);
        }

        public async Task<BaseResponse<ProductPagedResultDto>> GetFilteredPagedAsync(long? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 12;

            Expression<Func<Product, bool>> predicate = p =>
                (!categoryId.HasValue || p.CategoryId == categoryId.Value) &&
                (!minPrice.HasValue || p.Price >= minPrice.Value) &&
                (!maxPrice.HasValue || p.Price <= maxPrice.Value);

            var total = await _productRepo.CountAsync(predicate);
            var skip = (page - 1) * pageSize;
            var normalizedSort = sortBy?.Trim().ToLowerInvariant();

            Func<IQueryable<Product>, IOrderedQueryable<Product>> orderBy = normalizedSort == "bestsellers"
                ? q => q.OrderByDescending(p => p.SoldCount).ThenByDescending(p => p.CreatedAt)
                : q => q.OrderByDescending(p => p.CreatedAt);

            var items = await _productRepo.GetAsync(
                predicate: predicate,
                orderBy: orderBy,
                include: q => q.Include(p => p.Category),
                disableTracking: true,
                skip: skip,
                take: pageSize);

            var dto = new ProductPagedResultDto
            {
                Items = items.Select(ToListDto).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize)
            };

            return BaseResponse<ProductPagedResultDto>.Success(dto);
        }

        public async Task<BaseResponse<ProductListDto>> GetByIdAsync(long id)
        {
            var p = await _productRepo.Query(x => x.Id == id)
                .Include(x => x.Category)
                .FirstOrDefaultAsync();
            if (p == null)
                return BaseResponse<ProductListDto>.Fail("Product not found");

            return BaseResponse<ProductListDto>.Success(ToListDto(p));
        }

        public async Task<BaseResponse<List<ProductListDto>>> GetByCategoryAsync(long categoryId)
        {
            var products = await _productRepo.GetFilteredAsync(categoryId, null, null);
            var dtoList = products.Select(ToListDto).ToList();

            return BaseResponse<List<ProductListDto>>.Success(dtoList);
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
                // swallow cleanup issues
            }
        }
    }
}
