using Hadiyah.Attributes;
using HadiyahServices.DTOs.Product;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hadiyah.Controllers
{
    [AdminAuthorize] // only admins
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public AdminProductController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllAsync();
            return View(products.Data);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto dto, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                dto.ImageBase64 = Convert.ToBase64String(ms.ToArray());
            }

            var result = await _productService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ModelState.AddModelError("", result.Error);
                return View(dto);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (!product.IsSuccess)
                return RedirectToAction("Index");

            ViewBag.Categories = await _categoryService.GetAllAsync();

            var dto = new ProductUpdateDto
            {
                Id = product.Data.Id,
                Name = product.Data.Name,
                Description = product.Data.Description,
                Price = product.Data.Price,
                ImageBase64 = product.Data.ImageBase64,
                StockQuantity = product.Data.StockQuantity,
                CategoryId = product.Data.CategoryId,
                IsActive = product.Data.IsActive
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductUpdateDto dto, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                dto.ImageBase64 = Convert.ToBase64String(ms.ToArray());
            }

            var result = await _productService.UpdateAsync(dto);

            if (!result.IsSuccess)
            {
                ViewBag.Categories = await _categoryService.GetAllAsync();
                ModelState.AddModelError("", result.Error);
                return View(dto);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (!product.IsSuccess)
                return RedirectToAction("Index");

            return View(product.Data);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
