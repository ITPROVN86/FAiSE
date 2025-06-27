using FAiSEBussiness.Models;
using FAiSEWebClient.Models;
using Microsoft.EntityFrameworkCore;

namespace FAiSEWebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Thêm dịch vụ cho Dependency Injection
            builder.Services.Configure<BlogSettings>(
    builder.Configuration.GetSection("BlogSettings"));
            // Đăng ký DbContext với Dependency Injection
            builder.Services.AddDbContext<FaiSeContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("FAiSEDB")));

            // Add services to the container.
            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute("/Client/Detail", "{slug}_{id:int}");
                options.Conventions.AddPageRoute("/Categories/Detail", "Category/{slug}_{id:int}");
            });

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
