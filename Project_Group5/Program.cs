using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Project_Group5.Models;

namespace Project_Group5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Đăng ký DbContext với chuỗi kết nối
            builder.Services.AddDbContext<Fall24_SE1745_PRN221_Group5Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));

            // Đăng ký Razor Pages và các dịch vụ cần thiết
            builder.Services.AddRazorPages();

            // Thêm dịch vụ Session
            builder.Services.AddDistributedMemoryCache(); // Cần thiết cho Session
           builder.Services.AddSession(options =>
           {
               options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn của session
               options.Cookie.HttpOnly = true;
               options.Cookie.IsEssential = true; // Bắt buộc cookie của Session để hoạt động
           });


            // Cấu hình Authentication sử dụng Cookie
          //  builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

            })
                .AddCookie(
                options =>
                {
                    options.LoginPath = "/Login"; // Đường dẫn đến trang đăng nhập
                   options.LogoutPath = "/Logout"; // Đường dẫn để đăng xuất
                    options.AccessDeniedPath = "/AccessDenied"; // Đường dẫn khi không có quyền truy cập
                })
                .AddGoogle(GoogleDefaults.AuthenticationScheme,options =>
                {
                    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
                    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
                    options.CallbackPath = "/signin-google";
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

            app.UseSession();

            // Thêm Authentication và Authorization vào pipeline
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapRazorPages();

            app.Run();
        }
    }
}
