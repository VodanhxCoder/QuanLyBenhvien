using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Fontend.Services
{
    public class ReCaptchaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public ReCaptchaService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _secretKey = configuration["ReCaptcha:SecretKey"];
        }

        public async Task<bool> VerifyCaptchaAsync(string captchaResponse)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret", _secretKey },
            { "response", captchaResponse }
        });

            var response = await _httpClient.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(jsonResponse);
                var root = document.RootElement;
                return root.GetProperty("success").GetBoolean();
            }

            return false;
        }
    }
    }
