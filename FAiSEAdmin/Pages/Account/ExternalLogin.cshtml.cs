using FAiSEBussiness.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace FAiSEAdmin.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly FaiSeContext _context;
        public ExternalLoginModel(FaiSeContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string provider)
        {
            var redirectUrl = Url.Page("/Account/ExternalLoginCallback", pageHandler: null, values: null, protocol: Request.Scheme);
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }
    }
}
