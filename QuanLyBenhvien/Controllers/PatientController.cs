using Microsoft.AspNetCore.Mvc;

namespace HospitalFrontend.Controllers
{
    public class PatientController : Controller
    {
        public IActionResult Index()
        {
            var userType = HttpContext.Session.GetString("UserType");
            if (userType != "Patient")
            {
                return RedirectToAction("Index", "Home", new { error = "Access denied. Patients only." });
            }
            return View();
        }
    }
}