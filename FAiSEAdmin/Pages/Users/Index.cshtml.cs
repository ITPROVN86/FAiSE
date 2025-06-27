using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authorization;

namespace FAiSEAdmin.Pages.Users
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly FaiSeContext _context;

        public IndexModel(FaiSeContext context)
        {
            _context = context;
        }

        public IList<User> User { get;set; } = default!;
        public int TotalPages { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; } // Từ khóa tìm kiếm

        public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (_context.Users != null)
            {

                // Khởi tạo truy vấn
                var query = _context.Users.AsQueryable();

                // Nếu có từ khóa tìm kiếm, áp dụng bộ lọc
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(s =>
                        EF.Functions.Like(s.FullName, $"%{SearchTerm}%")
                    );
                }

                // Đếm tổng số bản ghi sau bộ lọc
                int total = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(total / (double)pageSize);
                CurrentPage = pageNumber;

                // Lấy dữ liệu cho trang hiện tại
                User = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại!";
                return RedirectToPage("./Index");
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Người dùng đã được xóa!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa người dùng vì nó có dữ liệu liên quan!";
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

     
                user.Status = !user.Status;
    

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, newValue = user.Status });
        }
    }
}
