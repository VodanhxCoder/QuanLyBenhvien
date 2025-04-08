using Microsoft.AspNetCore.Mvc;

namespace HospitalFrontend.Controllers
{
    public class DoctorController : Controller
    {
        public IActionResult Index()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Doctor")
            {
                return RedirectToAction("Index", "Home", new { error = "Access denied. Doctors only." });
            }
            return View();
        }
    }
}