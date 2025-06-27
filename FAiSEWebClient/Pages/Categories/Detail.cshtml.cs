using FAiSEBussiness.Models;
using FAiSEWebClient.AppCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FAiSEWebClient.Pages.Categories
{
    public class CategoryDetailModel : PageModel
    {
        private readonly FaiSeContext _context;

        public CategoryDetailModel(FaiSeContext context)
        {
            _context = context;
        }
        public Category? Category { get; set; }
        public List<Blog> Blogs { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public List<(string Label, string Url)> Breadcrumb { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string slug, int id, int p = 1)
        {
            const int pageSize = 9;
            CurrentPage = p;

            Category = await _context.Categories.FindAsync(id);

            if (Category == null)
                return NotFound();

            var expectedSlug = SlugHelper.GenerateSlug(Category.CategoryName);
            if (!string.Equals(expectedSlug, slug, StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToPagePermanent("Detail", new { slug = expectedSlug, id = id });
            }

            IQueryable<Blog> query;

            /*var query = _context.Blogs
                   .Where(b => b.CategoryId == id && b.Status)
                   .OrderByDescending(b => b.DateUpdated);*/

            // Nếu là danh mục cha (không có cha)
            var isParentCategory = await _context.Categories.AnyAsync(c => c.ParentCategoryId == id);
            if (isParentCategory)
            {
                var subCategoryIds = await _context.Categories
                                        .Where(c => c.ParentCategoryId == id && c.Status)
                                        .Select(c => c.CategoryId)
                                        .ToListAsync();

                query = _context.Blogs
                        .Where(b => subCategoryIds.Contains(b.CategoryId) && b.Status).OrderByDescending(b => b.DateUpdated);
            }
            else
            {
                // Nếu là danh mục con bình thường
                query = _context.Blogs
                        .Where(b => b.CategoryId == id && b.Status).OrderByDescending(b => b.DateUpdated);
            }

            var totalItems = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            Blogs = await query
                    .Skip((p - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            // Tạo breadcrumb
            Breadcrumb.Add(("Trang chủ", "/"));
            //Đây không là cha
            if (Category?.ParentCategoryId != null)
            {
                var parent = await _context.Categories.FindAsync(Category.ParentCategoryId);
                if (parent != null)
                {
                    Breadcrumb.Add((parent.CategoryName, Url.Page("/Categories/Detail", new
                    {
                        slug = SlugHelper.GenerateSlug(parent.CategoryName),
                        id = parent.CategoryId
                    })));
                }
            }

            Breadcrumb.Add((Category.CategoryName, null)); // danh mục hiện tại (không có URL)

            return Page();
        }
    }
}
