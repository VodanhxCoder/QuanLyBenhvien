using Backend_API.Data;
using Backend_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationController : ControllerBase
    {
        private readonly HospitalManagementDbContext _context;

        public VerificationController(HospitalManagementDbContext context)
        {
            _context = context;
        }

        [HttpPost("SaveCode")]
        public async Task<IActionResult> SaveCode([FromBody] VerificationCode verificationCode)
        {
            // Xóa các mã cũ cho email này (nếu có)
            var oldCodes = await _context.VerificationCodes
                .Where(vc => vc.Email == verificationCode.Email && !vc.IsUsed)
                .ToListAsync();
            _context.VerificationCodes.RemoveRange(oldCodes);

            // Lưu mã mới
            _context.VerificationCodes.Add(verificationCode);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("VerifyCode")]
        public async Task<IActionResult> VerifyCode(string email, string code)
        {
            var verificationCode = await _context.VerificationCodes
                .FirstOrDefaultAsync(vc => vc.Email == email && vc.Code == code && !vc.IsUsed);

            if (verificationCode == null)
            {
                return Ok(false);
            }

            if (verificationCode.ExpiresAt < DateTime.UtcNow)
            {
                return Ok(false);
            }

            // Đánh dấu mã đã sử dụng
            verificationCode.IsUsed = true;
            await _context.SaveChangesAsync();

            return Ok(true);
        }
    }
}
