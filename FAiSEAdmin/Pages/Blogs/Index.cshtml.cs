using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authorization;

namespace FAiSEAdmin.Pages.Blogs
{
    [Authorize(Policy = "AdminOnly")]
    public class IndexModel : PageModel
    {
        private readonly FaiSeContext _context;

        public IndexModel(FaiSeContext context)
        {
            _context = context;
            GetParentCategories();
            Categories = _context.Categories.ToList();
        }

        public IList<Blog> Blog { get;set; } = default!;
        public int TotalPages { get; set; }
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; }
      

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; } // Từ khóa tìm kiếm

        public List<Category> Categories { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? ParentCategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedCategoryId { get; set; }
        public IEnumerable<Category> ParentCategories { get; set; } = new List<Category>();

        public async Task<IActionResult> OnGetChildCategoriesAsync(int parentId)
        {
            var childCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            return new JsonResult(childCategories);
        }

        public async Task GetParentCategories()
        {
            ParentCategories = _context.Categories
                .Where(c => c.SubCategoryStatus == false);
        }
        public List<Category> ChildCategories { get; set; } = new List<Category>();
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (_context.Blogs != null)
            {
                // Khởi tạo truy vấn
                var query = _context.Blogs
                    .Include(b => b.Category)  // Nạp danh mục để tránh lỗi truy vấn
                    .Include(b => b.User)      // Nạp người dùng
                    .OrderByDescending(d=>d.DateUpdated)
                    .AsQueryable();

                // Lọc theo danh mục cha
                if (ParentCategoryId.HasValue && ParentCategoryId.Value > 0)
                {
                    query = query.Where(b => b.Category.ParentCategoryId == ParentCategoryId);
                }
   

                // Lọc theo danh mục con
                if (SelectedCategoryId.HasValue && SelectedCategoryId.Value > 0)
                {
                    query = query.Where(b => b.CategoryId == SelectedCategoryId);
                }

                // Lọc theo tiêu đề bài viết
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(s => EF.Functions.Like(s.Title, $"%{SearchTerm}%"));
                }

                // Lấy danh sách danh mục con theo danh mục cha đã chọn
                if (ParentCategoryId.HasValue && ParentCategoryId.Value > 0)
                {
                    ChildCategories = await _context.Categories
                        .Where(c => c.ParentCategoryId == ParentCategoryId)
                        .ToListAsync();
                }
                else
                {
                    ChildCategories = new List<Category>(); // Reset nếu không có danh mục cha
                }

                // Đếm tổng số bản ghi sau khi lọc
                int total = await query.CountAsync();
                TotalPages = (int)Math.Ceiling(total / (double)pageSize);
                CurrentPage = pageNumber;

                // Lấy dữ liệu cho trang hiện tại (có phân trang)
                Blog = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .OrderByDescending(d=>d.DateUpdated)
                    .ToListAsync();
            }

            return Page();
        }

        public string GetCategoryHierarchy(Category category)
        {
            if (category == null) return string.Empty;

            if (category.ParentCategoryId == null)
            {
                return category.CategoryName; // Nếu không có cha, trả về chính nó
            }

            var parent = Categories.FirstOrDefault(c => c.CategoryId == category.ParentCategoryId);
            if (parent != null)
            {
                return GetCategoryHierarchy(parent) + " > " + category.CategoryName;
            }

            return category.CategoryName;
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null)
            {
                TempData["ErrorMessage"] = "Chuyên mục không tồn tại!";
                return RedirectToPage("./Index");
            }

            try
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Chuyên mục đã được xóa!";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Không thể xóa chuyên mục vì nó có dữ liệu liên quan!";
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
                category.Status = !category.Status;
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true, newValue =category.Status });
        }
    }
}
