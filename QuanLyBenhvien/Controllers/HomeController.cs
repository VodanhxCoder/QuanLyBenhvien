using Fontend.Services;
using HospitalFrontend.Models;
using HospitalFrontend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalFrontend.Controllers
{
    public class HomeController : Controller
    {
     

        //Inject AuthService vào HomeController
        // Khi khởi tạo HomeController, AuthService sẽ được khởi tạo 
      private readonly AuthService _authService;
    private readonly ReCaptchaService _reCaptchaService;

    public HomeController(AuthService authService, ReCaptchaService reCaptchaService)
    {
        _authService =   authService;
        _reCaptchaService = reCaptchaService;
    }


        
        public IActionResult Index() //Auto chuyển hướng từ trang chủ đến các trang khác 
        {
            // Nếu đã đăng nhập, điều hướng theo vai trò
            var userType = HttpContext.Session.GetString("UserType");
            if (!string.IsNullOrEmpty(userType))
            {
                return userType switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Patient" => RedirectToAction("Index", "Patient"),
                    "Doctor" => RedirectToAction("Index", "Doctor"),
                    _ => View()
                };
            }
            return View();
        }

        public IActionResult Login() //Auto chuyển hướng từ trang chủ đến các trang khác
        {
            // Nếu đã đăng nhập, điều hướng theo vai trò
            var userType = HttpContext.Session.GetString("UserType");
            if (!string.IsNullOrEmpty(userType))
            {
                //Xác Định userType và điều hướng đến trang tương ứng
                return userType switch 
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Patient" => RedirectToAction("Index", "Patient"),
                    "Doctor" => RedirectToAction("Index", "Doctor"), 
                    _ => View() // Trả về view đăng nhập nếu không xác định được userType
                };
            }
            return View(new LoginDto()); // Trả về view đăng nhập với LoginDto rỗng
        }


        // Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken] //Chống CSRF
        public async Task<IActionResult> Login(LoginDto loginDto) 
        {
            if (!ModelState.IsValid) // Kiểm tra tính hợp lệ của model
            {
                // Nếu model không hợp lệ, trả về view với thông báo lỗi
             
                return View(loginDto);
            }

            // Kiểm tra reCAPTCHA
            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(captchaResponse))
            {
                ViewBag.Error = "Vui lòng xác minh bạn không phải là robot.";
                return View(loginDto);
            }

            var isCaptchaValid = await _reCaptchaService.VerifyCaptchaAsync(captchaResponse);
            if (!isCaptchaValid)
            {
                ViewBag.Error = "Xác minh reCAPTCHA không thành công. Vui lòng thử lại.";
                return View(loginDto);
            }


            try
            {
                var authResponse = await _authService.LoginAsync(loginDto); // Gọi AuthService để thực hiện đăng nhập 

                //nhận được AuthResponseDto từ backend (token, username, usertype)


                // Lưu userType vào session
                HttpContext.Session.SetString("UserType", authResponse.UserType);

                // Điều hướng dựa trên userType
                return authResponse.UserType switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Patient" => RedirectToAction("Index", "Patient"),
                    "Doctor" => RedirectToAction("Index", "Doctor"),
                    _ => RedirectToAction("Index", new { error = "Unknown user type" })
                };
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(loginDto);
            }
        }

        //-- Xử lý login qua socical Login 

        // Action để bắt đầu đăng nhập bằng Social Login
        public IActionResult ExternalLogin(string provider)
        {
            Console.WriteLine($"ExternalLogin called with provider: {provider}");
            Console.WriteLine("Post ok ");
            var redirectUrl = Url.Action("ExternalLoginCallback", "Home", null, Request.Scheme);
            Console.WriteLine($"Redirect URL: {redirectUrl}"); //
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }
        // Action xử lý callback từ Social Login
        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var info = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!info.Succeeded)
            {
                ViewBag.Error = "Đăng nhập bằng tài khoản xã hội thất bại.";
                return View("Login");
            }

            // Lấy thông tin từ nhà cung cấp (email, name, v.v.)
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);
            var provider = info.Principal.Identity.AuthenticationType;

            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Không thể lấy email từ tài khoản xã hội.";
                return View("Login");
            }

            // Gọi API backend để đăng nhập hoặc tạo tài khoản
            try
            {
                var loginDto = new ExternalLoginDto
                {
                    Email = email,
                    Provider = provider
                };

                var authResponse = await _authService.ExternalLoginAsync(loginDto);
                HttpContext.Session.SetString("UserType", authResponse.UserType);
                Response.Cookies.Append("token", authResponse.Token, new CookieOptions { HttpOnly = true });

                return authResponse.UserType switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "Patient" => RedirectToAction("Index", "Patient"),
                    "Doctor" => RedirectToAction("Index", "Doctor"),
                    _ => RedirectToAction("Index", new { error = "Unknown user type" })
                };
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View("Login");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("token");
            return RedirectToAction("Index");
        }
    }
}
