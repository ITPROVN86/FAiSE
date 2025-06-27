using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FAiSEWebClient.ViewComponents
{
    public class NavbarViewComponent: ViewComponent
    {
        private readonly FaiSeContext _context;

        public NavbarViewComponent(FaiSeContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var parents = await _context.Categories
                .Where(c => c.ParentCategoryId == null && c.Status && c.ShowMenuStatus)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    LinkUrl = c.LinkUrl,
                }).ToListAsync();

            foreach (var parent in parents)
            {
                parent.InverseParentCategory = await _context.Categories
                    .Where(c => c.ParentCategoryId == parent.CategoryId && c.Status && c.ShowMenuStatus)
                    .Select(c => new Category
                    {
                        CategoryId = c.CategoryId,
                        CategoryName = c.CategoryName,
                        LinkUrl = c.LinkUrl,
                    }).ToListAsync();
            }

            return View("../../Client/_Navbar", parents); // dùng view như partial
        }
    }
}
