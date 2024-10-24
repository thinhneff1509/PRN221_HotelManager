using Microsoft.EntityFrameworkCore;
using Project_Group5.Models;
using Project_Group5.Repositoríes;
using Project_Group5.Repositoríes.Interfaces;

namespace Project_Group5
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register your DbContext with connection string
            builder.Services.AddDbContext<Fall24_SE1745_PRN221_Group5Context>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}