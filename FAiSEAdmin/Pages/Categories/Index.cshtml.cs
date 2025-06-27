using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authorization;

namespace FAiSEAdmin.Pages.Categories
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly FaiSeContext _context;

        public IndexModel(FaiSeContext context)
        {
            _context = context;
        }

        public IList<Category> Category { get;set; } = default!;
        public int TotalPages { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; } // Từ khóa tìm kiếm

        public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (_context.Categories != null)
            {
                // Khởi tạo truy vấn
                var query = _context.Categories.AsQueryable();

                // Nếu có từ khóa tìm kiếm, áp dụng bộ lọc
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(s =>
                        EF.Functions.Like(s.CategoryName, $"%{SearchTerm}%")
                    );
                }

                // Đếm tổng số bản ghi sau bộ lọc
                int total = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(total / (double)pageSize);
                CurrentPage = pageNumber;

                // Lấy dữ liệu cho trang hiện tại
                Category = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Danh mục không tồn tại!";
                return RedirectToPage("./Index");
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Danh mục đã được xóa!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục vì nó có dữ liệu liên quan!";
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id, string field)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            if (field == "SubCategoryStatus")
            {
                category.SubCategoryStatus = !category.SubCategoryStatus;
            }
            else if (field == "ShowMenuStatus")
            {
                category.ShowMenuStatus = !category.ShowMenuStatus;
            }
            else if (field == "Status")
            {
                category.Status = !category.Status;
            }

            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, newValue = field == "SubCategoryStatus" ? category.SubCategoryStatus : field == "ShowMenuStatus" ? category.ShowMenuStatus : category.Status });
        }
    }
}
