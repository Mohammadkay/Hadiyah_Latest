using HadiyahServices.DTOs.Category;
using HadiyahServices.DTOs.Product;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Hadiyah.Controllers
{
    [AllowAnonymous]
    public class ShopController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public ShopController(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        public async Task<IActionResult> Categories()
        {
            var categoriesResponse = await _categoryService.GetAllAsync();
            var categories = categoriesResponse.Data ?? Enumerable.Empty<CategoryListDto>();
            ViewBag.Title = "Categories";
            return View(categories);
        }

        public async Task<IActionResult> Index(long? categoryId, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1, int pageSize = 12)
        {
            sortBy = string.IsNullOrWhiteSpace(sortBy) ? "newest" : sortBy.Trim().ToLowerInvariant();
            var categoriesResponse = await _categoryService.GetAllAsync();
            var productsResponse = await _productService.GetFilteredPagedAsync(categoryId, minPrice, maxPrice, sortBy, page, pageSize);

            var categories = categoriesResponse.Data ?? Enumerable.Empty<CategoryListDto>();
            var products = productsResponse.Data ?? new ProductPagedResultDto
            {
                Items = Enumerable.Empty<ProductListDto>(),
                Page = page,
                PageSize = pageSize
            };

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.PageSize = products.PageSize;
            ViewBag.Page = products.Page;
            ViewBag.TotalPages = products.TotalPages;
            ViewBag.TotalCount = products.TotalCount;
            ViewBag.Title = "Shop";

            return View(products);
        }

        public async Task<IActionResult> Products(long categoryId, int page = 1, int pageSize = 12)
        {
            var products = await _productService.GetFilteredPagedAsync(categoryId, null, null, "newest", page, pageSize);

            if (!products.IsSuccess || products.Data == null)
            {
                TempData["ShopMessage"] = "We couldn't find that category.";
                TempData["ShopMessageType"] = "warning";
                return RedirectToAction("Index");
            }

            var list = (products.Data.Items ?? Enumerable.Empty<ProductListDto>()).ToList();
            var categoryName = list.FirstOrDefault()?.CategoryName;

            ViewBag.CategoryId = categoryId;
            ViewBag.ProductHeading = string.IsNullOrWhiteSpace(categoryName) ? "Available Gifts" : $"{categoryName} Gifts";
            ViewBag.ProductSubtitle = list.Any()
                ? $"{products.Data.TotalCount} curated item(s) ready to ship"
                : "No products available in this category yet.";

            ViewBag.Title = ViewBag.ProductHeading;
            ViewBag.PageAction = "Products";

            return View(products.Data);
        }

        public async Task<IActionResult> AllProducts(int page = 1, int pageSize = 12)
        {
            var response = await _productService.GetFilteredPagedAsync(null, null, null, "newest", page, pageSize);
            var model = response.Data ?? new ProductPagedResultDto
            {
                Page = page,
                PageSize = pageSize
            };
            var list = (model.Items ?? Enumerable.Empty<ProductListDto>()).ToList();

            ViewBag.CategoryId = null;
            ViewBag.ProductHeading = "All Products";
            ViewBag.ProductSubtitle = list.Any()
                ? $"{model.TotalCount} curated item(s) across categories"
                : "No products available yet.";
            ViewBag.Title = "All Products";
            ViewBag.PageAction = "AllProducts";

            return View("Products", model);
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var result = await _productService.GetByIdAsync(id);

            if (!result.IsSuccess || result.Data == null)
            {
                return RedirectToAction("Index");
            }

            return View(result.Data); // ProductListDto
        }
    }
}
