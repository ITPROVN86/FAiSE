using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authorization;

namespace FAiSEAdmin.Pages.Categories
{
    [Authorize(Policy = "AdminOnly")]
    public class EditModel : PageModel
    {
        private readonly FaiSeContext _context;

        public EditModel(FaiSeContext context)
        {
            _context = context;
        }

        public IEnumerable<Category> ParentCategories { get; set; }

        [BindProperty]
        public Category Category { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category =  await _context.Categories.FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }
            Category = category;
            //ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            GetParentCategories();
            return Page();
        }

        public async Task GetParentCategories()
        {
            ParentCategories = _context.Categories
                .Where(c => c.SubCategoryStatus == false);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
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
            _context.Attach(Category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(Category.CategoryId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CategoryExists(int id)
        {
          return (_context.Categories?.Any(e => e.CategoryId == id)).GetValueOrDefault();
        }
    }
}
