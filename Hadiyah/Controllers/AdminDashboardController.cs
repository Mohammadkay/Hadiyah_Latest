using Hadiyah.Attributes;
using HadiyahDomain.enums;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Hadiyah.Controllers
{
    [AdminAuthorize]
    public class AdminDashboardController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;

        public AdminDashboardController(
            IUserRepository userRepo,
            ICategoryRepository categoryRepo,
            IProductRepository productRepo,
            IOrderRepository orderRepo
            )
        {
            _userRepo = userRepo;
            _categoryRepo = categoryRepo;
            _productRepo = productRepo;
            _orderRepo = orderRepo;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userRepo.CountAsync(),
                TotalCategories = await _categoryRepo.CountAsync(),
                TotalProducts = await _productRepo.CountAsync(),
                TotalOrders = await _orderRepo.CountAsync(),
                PendingOrders = (await _orderRepo.FindAsync(o => o.Status == OrderStatus.Pending)).Count(),
                Revenue = (await _orderRepo.GetAllAsync()).Sum(o => o.TotalAmount)
            };

            return View(model);
        }
    }
}
