using Fontend.Services;
using Fontend.Services.Gmail;
using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);




//Buider for FB and GG
// Đọc cấu hình từ appsettings.json
var configuration = builder.Configuration;


//
// Debug: In tất cả các key trong configuration để kiểm tra
foreach (var key in builder.Configuration.AsEnumerable())
{
    Console.WriteLine($"Configuration Key: {key.Key}, Value: {key.Value}");
}

// Đọc cấu hình BackendApiUrl
var backendApiUrl = builder.Configuration["BackendApiUrl"];
Console.WriteLine($"BackendApiUrl: {backendApiUrl}");

// Kiểm tra BackendApiUrl
if (string.IsNullOrEmpty(backendApiUrl))
{
    Console.WriteLine("Error: BackendApiUrl is not configured in appsettings.json. Using default value.");
    backendApiUrl = "https://localhost:7023/";
}
//

// Thêm dịch vụ Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; 
})

.AddCookie()
.AddFacebook(options =>
{
    options.AppId = configuration["Authentication:Facebook:AppId"];
    options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
   
    options.CallbackPath = "/signin-facebook";
    options.Fields.Add("email");
    options.Fields.Add("name");
    options.Events.OnRemoteFailure = context =>
    {
        Console.WriteLine($"Facebook authentication failed: {context.Failure?.Message}");
        context.Response.Redirect("/Home/Login?error=" + Uri.EscapeDataString("Đăng nhập bằng Facebook thất bại: " + context.Failure?.Message));
        context.HandleResponse();
        return Task.CompletedTask;
    };
})
.AddGoogle(options =>
{
    options.ClientId = configuration["Authentication:Google:ClientId"];
    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Events.OnRemoteFailure = context =>
    {
        Console.WriteLine($"Google authentication failed: {context.Failure?.Message}");
        context.Response.Redirect("/Home/Login?error=" + Uri.EscapeDataString("Đăng nhập bằng Google thất bại: " + context.Failure?.Message));
        context.HandleResponse();
        return Task.CompletedTask;
    };
});




// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpClient for calling backend API
builder.Services.AddHttpClient("BackendApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendApiUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});



//add Gmail Service (MailKit)
builder.Services.AddSingleton<EmailService>();



// Add VerificationService
builder.Services.AddHttpClient<VerificationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BackendApiUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});





// Add session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});



// Add AuthService
builder.Services.AddScoped<AuthService>();

//add ReCaptchaService
builder.Services.AddHttpClient<ReCaptchaService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();