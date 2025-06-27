using FAiSEBussiness.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FAiSEAdmin
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Khởi tạo Firebase
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(Path.Combine(builder.Environment.WebRootPath, "keys/firebase-key.json"))
            });

            // Đăng ký DbContext với Dependency Injection
            builder.Services.AddDbContext<FaiSeContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("FAiSEDB")));

            // Thêm dịch vụ xác thực Cookie
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //muốn người dùng truy cập trực tiếp đến trang đăng nhập
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;//GoogleDefaults.AuthenticationScheme;
            })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";  // Đường dẫn trang login
                    options.AccessDeniedPath = "/Account/Login"; // Trang báo lỗi khi bị từ chối truy cập
                })
                .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = "Client Id";
                    options.ClientSecret = "Secret";
                    options.CallbackPath = "/Account/signin-google"; // Mặc định URL callback từ Google
                });
            // Cấu hình Authorization
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Role", "Admin"));
            });
            builder.Services.AddDistributedMemoryCache(); // Sử dụng cache để lưu session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian timeout của session
                options.Cookie.HttpOnly = true; // Bảo mật hơn, tránh truy cập từ JavaScript
                options.Cookie.IsEssential = true; // Đảm bảo session hoạt động trong mọi trường hợp
            });
            //Đăng ký IHttpContextAccessor để lấy UserName từ Session trong Partial View,
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthorization();
            // Add services to the container.

            builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            builder.Services.AddRazorPages()
                .AddRazorPagesOptions(options =>
            {
                options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
            }).AddMvcOptions(options =>
            {
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    _ => "Trường này không được để trống.");
            });
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // Hỗ trợ upload file tối đa 10MB
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

            // Kích hoạt Session Middleware
            app.UseSession(); // QUAN TRỌNG: phải thêm dòng này

            app.UseRouting();

            app.UseAuthentication(); // Sử dụng xác thực
            app.UseAuthorization();  // Sử dụng phân quyền

            app.MapRazorPages();

            app.Run();
        }
    }
}
