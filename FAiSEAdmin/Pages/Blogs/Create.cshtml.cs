using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using FAiSEBussiness.Models;
using Microsoft.Extensions.Hosting;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using FirebaseAdmin;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;

namespace FAiSEAdmin.Pages.Blogs
{
    [Authorize(Policy = "AdminOnly")]
    public class CreateModel : BasePageModel
    {
        private readonly FaiSeContext _context;
        private readonly IWebHostEnvironment _environment;
        public CreateModel(FaiSeContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            this._environment = webHostEnvironment;
        }

        [BindProperty]
        public int? ParentCategoryId { get; set; }

        [BindProperty]
        public int? SelectedCategoryId { get; set; }
        public IEnumerable<Category> ParentCategories { get; set; } = new List<Category>();
        public async Task<IActionResult> OnGetAsync()
        {
            GetParentCategories();

            return Page();
        }
        public async Task GetParentCategories()
        {
            ParentCategories = _context.Categories
                .Where(c => c.SubCategoryStatus == false);
        }
        public async Task<IActionResult> OnGetChildCategoriesAsync(int parentId)
        {
            var childCategories = await _context.Categories
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToListAsync();

            return new JsonResult(childCategories);
        }

        [BindProperty]
        public Blog? Blog { get; set; } = new Blog();

        [BindProperty]
        public IFormFile? Upload { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            Blog.DateUpdated = Common.GetServerDateTime();
            /*       if (!ModelState.IsValid)
                   {
                       foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                       {
                           Console.WriteLine(error.ErrorMessage);
                       }
                       return Page();
                   }
                   if (!ModelState.IsValid || _context.Blogs == null || Blog == null)
                   {
                       return Page();
                   }*/

            // Nếu có ảnh mới được upload, lấy đường dẫn từ input ẩn
            if (!string.IsNullOrEmpty(Request.Form["AvatarPath"]))
            {
                Blog.Avatar = Request.Form["AvatarPath"];
            }

            // Nếu danh mục con có giá trị, sử dụng nó, nếu không lấy danh mục cha
            Blog.CategoryId = SelectedCategoryId ?? Convert.ToInt32(ParentCategoryId);


            var user = User as ClaimsPrincipal;
            var email = user?.FindFirstValue(ClaimTypes.Email);
            var query = _context.Users.FirstOrDefault(x => x.Mail == email);
            Blog.UserId = query.Id;

            SetAlert("Cập nhật Dữ liệu Thành công", "success");

            _context.Blogs.Add(Blog);
            await _context.SaveChangesAsync();
            GetParentCategories();
            return RedirectToPage("./Index");
        }


        public async Task<IActionResult> OnPostUploadAvatarAsync()
        {
            if (Upload == null || Upload.Length == 0)
                return BadRequest(new { success = false, error = "No file uploaded" });

            var storage = StorageClient.Create(FirebaseApp.DefaultInstance.Options.Credential);

            // Lấy bucket đã cấu hình
            var bucket = "faise-1f797.firebasestorage.app"; 

            // Tạo tên file duy nhất
            string uniqueFileName = $"{Guid.NewGuid()}_{Upload.FileName}";

            // Upload file lên Firebase
            using (var stream = Upload.OpenReadStream())
            {
                await storage.UploadObjectAsync(bucket, $"avatars/{uniqueFileName}", Upload.ContentType, stream);
            }

            // Tạo link download file từ Firebase
            var url = $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{Uri.EscapeDataString($"avatars/{uniqueFileName}")}?alt=media";

            return new JsonResult(new { success = true, filePath = url });

            /*if (Upload == null || Upload.Length == 0)
            {
                return BadRequest(new { success = false, error = "No file uploaded" });
            }

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "Uploads", "Avatar");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = $"{Guid.NewGuid()}_{Upload.FileName}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Upload.CopyToAsync(fileStream);
            }

            string fileUrl = $"/Uploads/Avatar/{uniqueFileName}";

            return new JsonResult(new { success = true, filePath = fileUrl });*/
        }

        public async Task<IActionResult> OnPostUploadImageAsync()
        {
            // Get the current date and format it
            //var currentDate = DateTime.Now;
            //var year = currentDate.Year.ToString();
            //var month = currentDate.Month.ToString().PadLeft(2, '0');
            //var day = currentDate.Day.ToString().PadLeft(2, '0');

            if (Upload == null || Upload.Length == 0)
                return BadRequest(new { success = false, error = "No file uploaded" });

            var storage = StorageClient.Create(FirebaseApp.DefaultInstance.Options.Credential);

            // Lấy bucket đã cấu hình
            var bucket = "faise-1f797.firebasestorage.app";

            // Tạo tên file duy nhất
            string uniqueFileName = $"{Guid.NewGuid()}_{Upload.FileName}";

            // Upload file lên Firebase
            using (var stream = Upload.OpenReadStream())
            {
                await storage.UploadObjectAsync(bucket, $"blogs/{uniqueFileName}", Upload.ContentType, stream);
            }

            // Tạo link download file từ Firebase
            var url = $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{Uri.EscapeDataString($"blogs/{uniqueFileName}")}?alt=media";
            ViewData["Anh"] = url;
            return new JsonResult(new { uploaded = true, url = url });
            //return new JsonResult(new { success = true, filePath = url });

        }
    }
}
