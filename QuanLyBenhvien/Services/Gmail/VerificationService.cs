using Fontend.Models;
using HospitalFrontend.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fontend.Services.Gmail
{
    public class VerificationService
    {
        private readonly HttpClient _httpClient;

        public VerificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Console.WriteLine($"VerificationService: BaseAddress = {_httpClient.BaseAddress?.ToString() ?? "null"}");
        }

        public async Task<string> GenerateAndSendVerificationCodeAsync(string email, EmailService emailService)
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            var verificationCode = new VerificationCode
            {
                Email = email,
                Code = code,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };

            var response = await _httpClient.PostAsJsonAsync("api/Verification/SaveCode", verificationCode);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Không thể lưu mã xác thực: {errorContent}");
            }

            await emailService.SendVerificationCodeAsync(email, code);
            return code;
        }
        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            var response = await _httpClient.GetAsync($"api/Verification/VerifyCode?email={email}&code={code}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return false;
            }

            return await response.Content.ReadFromJsonAsync<bool>();
        }
    }
}
