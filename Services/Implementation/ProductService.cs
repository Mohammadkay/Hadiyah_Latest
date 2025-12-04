using Domain.Entities;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Common;
using HadiyahServices.DTOs.Product;
using HadiyahServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HadiyahServices.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepo;
        private readonly ICategoryRepository _categoryRepo;

        public ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<BaseResponse<ProductListDto>> CreateAsync(ProductCreateDto dto)
        {
            if (await _productRepo.ExistsByNameAsync(dto.Name))
                return BaseResponse<ProductListDto>.Fail("Product name already exists");

            var category = await _categoryRepo.GetByIdAsync(dto.CategoryId);
            if (category == null)
                return BaseResponse<ProductListDto>.Fail("Invalid category");

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageBase64 = dto.ImageBase64,
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
                ImageBase64 = product.ImageBase64,
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
            product.ImageBase64 = dto.ImageBase64;
            product.StockQuantity = dto.StockQuantity;
            product.IsActive = dto.IsActive;
            product.CategoryId = dto.CategoryId;

            await _productRepo.UpdateAsync(product);

            return BaseResponse<ProductListDto>.Success(new ProductListDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageBase64 = product.ImageBase64,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = category.Name
            });
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                return BaseResponse<bool>.Fail("Product not found");

            await _productRepo.DeleteAsync(product);
            return BaseResponse<bool>.Success(true);
        }

        public async Task<BaseResponse<IEnumerable<ProductListDto>>> GetAllAsync()
        {
            var products = await _productRepo.GetAllAsync();

            var result = products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageBase64 = p.ImageBase64,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            });

            return BaseResponse<IEnumerable<ProductListDto>>.Success(result);
        }

        public async Task<BaseResponse<ProductListDto>> GetByIdAsync(int id)
        {
            var p = await _productRepo.GetByIdAsync(id);
            if (p == null)
                return BaseResponse<ProductListDto>.Fail("Product not found");

            return BaseResponse<ProductListDto>.Success(new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageBase64 = p.ImageBase64,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name
            });
        }

        public async Task<BaseResponse<IEnumerable<ProductListDto>>> GetByCategoryAsync(int categoryId)
        {
            var products = await _productRepo.GetByCategoryAsync(categoryId);

            var result = products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageBase64 = p.ImageBase64,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            });

            return BaseResponse<IEnumerable<ProductListDto>>.Success(result);
        }
    }
}
