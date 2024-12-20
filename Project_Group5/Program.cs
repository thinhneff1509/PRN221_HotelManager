﻿using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

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

            // Cấu hình Authentication sử dụng Cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login"; // Đường dẫn đến trang đăng nhập
                    options.LogoutPath = "/Logout"; // Đường dẫn để đăng xuất
                    options.AccessDeniedPath = "/AccessDenied"; // Đường dẫn khi không có quyền truy cập
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

            // Thêm middleware để chuyển hướng đến trang /home
    



            app.MapRazorPages();

            app.Run();
        }
    }
}
