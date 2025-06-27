using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace FAiSEAdmin.Pages
{
    [Authorize]
    public class IndexModel : BasePageModel
    {
        private readonly FaiSeContext _context;

        
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(FaiSeContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async void OnGetAsync()
        {
        
        }
    }
}
