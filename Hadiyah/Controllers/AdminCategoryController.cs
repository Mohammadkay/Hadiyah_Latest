using Hadiyah.Attributes;
using HadiyahServices.DTOs.Category;
using HadiyahServices.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hadiyah.Controllers
{
    [AdminAuthorize]
    public class AdminCategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _categoryService.GetAllAsync();
            return View(result.Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateDto dto, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (imageFile != null && imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                dto.ImageBase64 = Convert.ToBase64String(ms.ToArray());
            }

            var result = await _categoryService.CreateAsync(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return View(dto);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);

            if (!result.IsSuccess)
                return RedirectToAction("Index");

            var dto = new CategoryUpdateDto
            {
                Id = result.Data.Id,
                Name = result.Data.Name,
                ImagePath = result.Data.ImagePath,
                IsActive = result.Data.IsActive
            };

            return View(dto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryUpdateDto dto, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(dto);

            if (imageFile != null && imageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await imageFile.CopyToAsync(ms);
                dto.ImageBase64 = Convert.ToBase64String(ms.ToArray());
            }

            var result = await _categoryService.UpdateAsync(dto);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Error);
                return View(dto);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _categoryService.GetByIdAsync(id);
             
            if (!result.IsSuccess)
                return RedirectToAction("Index");

            return View(result.Data);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryService.DeleteAsync(id);
            return RedirectToAction("Index");//test
        }
    }
}
