using Hadiyah.Attributes;
using HadiyahDomain.enums;
using HadiyahRepositories.Interfaces;
using HadiyahServices.DTOs.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var pendingOrders = await _orderRepo.CountAsync(o => o.Status == OrderStatus.Pending);
            var revenue = await _orderRepo.Query().SumAsync(o => o.TotalAmount);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _userRepo.CountAsync(),
                TotalCategories = await _categoryRepo.CountAsync(),
                TotalProducts = await _productRepo.CountAsync(),
                TotalOrders = await _orderRepo.CountAsync(),
                PendingOrders = pendingOrders,
                Revenue = revenue
            };

            return View(model);
        }
    }
}
