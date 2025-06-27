using System.Text;
using FAiSEBussiness.Models;
using FAiSEWebClient.AppCode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FAiSEWebClient.Pages.Client
{
    public class DetailModel : PageModel
    {
        private readonly FaiSeContext _context;

        public DetailModel(FaiSeContext context)
        {
            _context = context;
        }
        public Blog BlogDetail { get; set; } = default!;
        public List<(string Label, string Url)> Breadcrumb { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(string slug, int id)
        {
            BlogDetail = await _context.Blogs.Include(c=>c.Category).ThenInclude(c => c.ParentCategory).FirstOrDefaultAsync(m => m.Id == id);

            if (BlogDetail == null)
            {
                return NotFound();
            }

            var expectedSlug = SlugHelper.GenerateSlug(BlogDetail.Title);
            if (slug != expectedSlug)
            {
                return RedirectToPagePermanent("Detail", new { slug = expectedSlug, id = id });
            }

            // Tạo breadcrumb
            Breadcrumb.Add(("Trang chủ", "/"));
            //Đây không là cha
            if (BlogDetail.Category?.ParentCategory != null)
            {
                var parent = BlogDetail.Category.ParentCategory;

                Breadcrumb.Add((parent.CategoryName, Url.Page("/Categories/Detail", new
                {
                    slug = SlugHelper.GenerateSlug(parent.CategoryName),
                    id = parent.CategoryId
                })));
            }

            Breadcrumb.Add((BlogDetail.Category.CategoryName, Url.Page("/Categories/Detail", new
            {
                slug = SlugHelper.GenerateSlug(BlogDetail.Category.CategoryName),
                id = BlogDetail.Category.CategoryId
            })));

            //Breadcrumb.Add((BlogDetail.Category.CategoryName, null)); // danh mục hiện tại (không có URL)

            return Page();
        }


    }
}
