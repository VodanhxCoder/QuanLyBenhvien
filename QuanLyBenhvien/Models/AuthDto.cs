using System.ComponentModel.DataAnnotations;

namespace HospitalFrontend.Models
{ //Model for login in the frontend, Can be used to send data to the backend
  //Use by HomeController 
    public class LoginDto
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất một chữ hoa, một chữ thường, một số và một ký tự đặc biệt")]
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
    }



    public class ExternalLoginDto
    {
        public string Email { get; set; }
        public string Provider { get; set; }
    }
}