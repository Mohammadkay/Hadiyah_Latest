using HadiyahServices.DTOs.enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Hadiyah.Attributes
{
    public class AdminAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity!.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "User", new { message = "Please login to continue." });
                return;
            }

            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == "RoleId")?.Value;

            if (!int.TryParse(roleClaim, out int roleId))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "User", new { message = "Unable to determine your role." });
                return;
            }

            if (roleId != (int)UserRole.Admin)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "User", new { message = "Admin privileges are required." });
            }
        }
    }
}
