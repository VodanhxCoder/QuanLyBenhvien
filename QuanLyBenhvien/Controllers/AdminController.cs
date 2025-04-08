using Microsoft.AspNetCore.Mvc;

namespace HospitalFrontend.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Admin")
            {
                return RedirectToAction("Index", "Home", new { error = "Access denied. Admins only." });
            }
            return View();
        }
    }
}