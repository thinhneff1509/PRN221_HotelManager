using DotNetEnv; // Thêm thư viện DotNetEnv
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;

namespace Project_Group5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Nạp biến môi trường từ tệp .env
            Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký DbContext với chuỗi kết nối
            builder.Services.AddDbContext<Fall24_SE1745_PRN221_Group5Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));

            // Đăng ký Razor Pages và các dịch vụ cần thiết
            builder.Services.AddRazorPages();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None; // Cấu hình này có thể cần thiết để đảm bảo cookie hoạt động với các yêu cầu OAuth
                options.LoginPath = "/Login"; // Đường dẫn đến trang đăng nhập
                options.LogoutPath = "/Logout"; // Đường dẫn để đăng xuất
                options.AccessDeniedPath = "/AccessDenied"; // Đường dẫn khi không có quyền truy cập
                /*            })
                            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                            {
                                // Lấy ClientId và ClientSecret từ biến môi trường
                                options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
                                options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

                                options.CallbackPath = "/signin-google";*/
            });

            // Cấu hình Authorization
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Cấu hình HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Thêm Authentication và Authorization vào pipeline
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
