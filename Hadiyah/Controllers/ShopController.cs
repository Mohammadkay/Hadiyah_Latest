using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories.Data);
        }

        public async Task<IActionResult> Products(int categoryId)
        {
            var products = await _productService.GetByCategoryAsync(categoryId);
            ViewBag.CategoryId = categoryId;
            return View(products.Data);
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
