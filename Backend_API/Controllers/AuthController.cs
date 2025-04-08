using Backend_API.Data;
using Backend_API.DTOs;
using Backend_API.DTOs.Backend_API.DTOs;
using Backend_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly HospitalManagementDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(HospitalManagementDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kiểm tra xem email đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already exists.");
            }

            // Tạo user mới
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Mã hóa mật khẩu
                UserType = dto.UserType
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }


        // Xử lý đăng nhập
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto) // nhận form gồm email and password 
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email); // Tìm người dùng theo email
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))   //kireerm tra xem người dùng có tồn tại không và mật khẩu có đúng không
            {
                // Nếu không tìm thấy người dùng hoặc mật khẩu không đúng, trả về lỗi
                return Unauthorized("Invalid email or password.");
            }
         
            var token = GenerateJwtToken(user);  // Tạo JWT token cho người dùng
            Response.Cookies.Append("token", token, new CookieOptions   // Lưu token vào cookie
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.AddHours(1) //  thời gian sống  1 giờ
            });

            return Ok(new { userName = user.UserName, userType = user.UserType });  // trả về thông tin người dùng (không bao gồm mật khẩu)
        }



        // Xử lý đăng nhập bằng tài khoản xã hội

        [HttpPost("ExternalLogin")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Provider))
            {
                return BadRequest("Thông tin đăng nhập không hợp lệ.");
            }

            // Kiểm tra xem email đã tồn tại trong hệ thống chưa
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
            {
                // Nếu email chưa tồn tại, trả về lỗi (sẽ xử lý đăng ký sau)
                return BadRequest("Tài khoản không tồn tại. Vui lòng đăng ký.");
            }

            // Tạo token JWT cho người dùng
            var token = GenerateJwtToken(user);

            // Lưu token vào cookie (tương tự như Login)
            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.Now.AddHours(1)
            });

            // Trả về thông tin người dùng
            return Ok(new AuthResponseDto
            {
                Token = token,
                UserName = user.UserName,
                UserType = user.UserType
            });
        }




        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()), // Lưu ID người dùng vào claim
        new Claim(ClaimTypes.Name, user.UserName), // Lưu tên người dùng vào claim
        new Claim(ClaimTypes.Role, user.UserType), // Lưu UserType (Admin, Patient, Doctor, v.v.) vào claim
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])); // Lấy khóa bí mật từ cấu hình
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Giảm thời gian sống xuống 1 giờ
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}