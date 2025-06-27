using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authorization;

namespace FAiSEAdmin.Pages.Categories
{
    [Authorize(Policy = "AdminOnly")]
    public class CreateModel : PageModel
    {
        private readonly FaiSeContext _context;

        public CreateModel(FaiSeContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> ParentCategories { get; set; }
        public async Task OnGetAsync()
        {
            /*ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
                return Page();*/
            GetParentCategories();
        }

        public async Task GetParentCategories()
        {
            ParentCategories = _context.Categories
                .Where(c => c.SubCategoryStatus == false);
        }

        [BindProperty]
        public Category Category { get; set; } = default!;
        public int? ParentCategoryId { get; set; } // Dùng nullable int

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _context.Categories == null || Category == null)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn danh mục cha.";
                return Page();
            }

            // Nếu danh mục con được chọn nhưng danh mục cha chưa được chọn, báo lỗi
            if (Category.SubCategoryStatus && string.IsNullOrEmpty(Category.ParentCategoryId?.ToString()))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn danh mục cha.";
                GetParentCategories();
                return Page();
            }

            // Nếu không phải danh mục con, reset ParentCategoryId để tránh lỗi
            if (!Category.SubCategoryStatus)
            {
                Category.ParentCategoryId = null;
            }

            try
            {
                _context.Categories.Add(Category);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi lưu danh mục: " + ex.Message;
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
