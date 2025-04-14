using HospitalFrontend.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Fontend.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        //Khi khởi tạo AuthService, HttpClient sẽ được khởi tạo từ IHttpClientFactory
        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }



  
        //Hàm này sẽ được gọi từ HomeController (Login) khi người dùng nhấn nút đăng nhập

        //đẩy LoginDto lên backend sau khi người dùng nhấn nút đăng nhập 
        //Nếu thành công, trả về AuthResponseDto
        //Nếu không thành công, ném ra HttpRequestException
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto) // được gọi từ HomeController (Login)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/Login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>(); //trả về AuthResponseDto gồm token, username, usertype
                return result;
            }
            throw new HttpRequestException("Invalid email or password.");
        }



        //Dược gọi từ HomeController  khi người dùng nhấn nút đăng nhập mạng xã hội
        public async Task<AuthResponseDto> ExternalLoginAsync(ExternalLoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/ExternalLogin", loginDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
                //trả về AuthResponseDto gồm token, username, usertype
            }

            throw new Exception("Đăng nhập bằng tài khoản xã hội thất bại.");
        }


        public async Task RegisterAsync(RegisterDto registerDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/Register", registerDto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Đăng ký thất bại: {error}");
            }
        }

    }
}