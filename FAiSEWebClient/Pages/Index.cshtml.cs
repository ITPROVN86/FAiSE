using FAiSEBussiness.Models;
using FAiSEWebClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FAiSEWebClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly FaiSeContext _context;

        private readonly BlogSettings _settings;

        public IndexModel(ILogger<IndexModel> logger, FaiSeContext context, IOptions<BlogSettings> settings)
        {
            _logger = logger;
            _context = context;
            _settings = settings.Value;
        }

        public IList<Blog> Blogs { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Blogs = await _context.Blogs.Where(x=>x.Status==true && _settings.HighlightCategoryIds.Contains(x.CategoryId))
                                  .OrderByDescending(b => b.DateUpdated)
                                  .Take(3)
                                  .ToListAsync();
        }
    }
}
