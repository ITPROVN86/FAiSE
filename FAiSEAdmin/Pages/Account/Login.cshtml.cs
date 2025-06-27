using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace FAiSEAdmin.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly FaiSeContext _context;

        public LoginModel(FaiSeContext context)
        {
            _context = context;
        }

        // Xử lý đăng nhập Google
        public async Task<IActionResult> OnPostGoogleLoginAsync()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Page("/Account/Login", new { handler = "GoogleLoginCallback" }) // Đúng Razor Page
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // Xử lý khi Google trả về dữ liệu sau khi login
        public async Task<IActionResult> OnGetGoogleLoginCallbackAsync()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return RedirectToPage("/Account/Login");
            }

            var googleId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

            if (googleId == null || email == null)
            {
                TempData["ErrorMessage"] = "Cái tài khoản email này không có giá trị!";
                return RedirectToPage("/Account/Login");
            }

            // Kiểm tra xem user đã tồn tại trong database chưa
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Mail == email);

            if (user == null)
            {
                //SetAlert("Tài khoản không tồn tại trong hệ thống", "error");
                TempData["ErrorMessage"] = "Tài khoản không tồn tại trong hệ thống!";
                return RedirectToPage("/Account/Login"); // Quay lại trang Login
            }

            // Lưu tên vào Session - Server-side
            HttpContext.Session.SetString("UserName", user.FullName);
            var claims = new List<Claim>
{
    new Claim(ClaimTypes.Name, user.FullName), // Lưu tên người dùng
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // Lưu UserId
    new Claim(ClaimTypes.Email, user.Mail),
    new Claim("Role", "Admin") // Lưu vai trò của User
};
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            //Client-side
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            return RedirectToPage("../Index"); // Chuyển hướng sau khi đăng nhập thành công
        }

        //public IActionResult OnGet()
        //{
           
        //}
    }
}
