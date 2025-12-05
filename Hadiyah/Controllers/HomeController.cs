using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Hadiyah.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var path = feature?.Path ?? string.Empty;

            TempData["Alert.Title"] = "Something went wrong";
            TempData["Alert.Type"] = "error";
            TempData["Alert.Message"] = "An unexpected error occurred. Please try again.";

            ViewBag.Path = path;
            return View();
        }
    }
}
