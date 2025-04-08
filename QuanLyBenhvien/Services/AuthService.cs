using HospitalFrontend.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HospitalFrontend.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;

        //Khi khởi tạo AuthService, HttpClient sẽ được khởi tạo từ IHttpClientFactory
        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendApi");
        }


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




        public async Task<AuthResponseDto> ExternalLoginAsync(ExternalLoginDto loginDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/ExternalLogin", loginDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            }
            throw new Exception("Đăng nhập bằng tài khoản xã hội thất bại.");
        }
    }
}