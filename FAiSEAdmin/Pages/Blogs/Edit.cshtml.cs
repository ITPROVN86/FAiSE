using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FAiSEBussiness.Models;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authorization;

namespace FAiSEAdmin.Pages.Blogs
{
    [Authorize(Policy = "AdminOnly")]
    public class EditModel : BasePageModel
    {
        private readonly FaiSeContext _context;
        private readonly IWebHostEnvironment _environment;
        public EditModel(FaiSeContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Blog Blog { get; set; } = new Blog();

        [BindProperty]
        public IFormFile? Upload { get; set; }
        public IEnumerable<Category> ParentCategories { get; set; } = new List<Category>();
        public List<Category> ChildCategories { get; set; } = new List<Category>();

        [BindProperty]
        public int ParentCategoryId { get; set; }

        [BindProperty]
        public int? SelectedCategoryId { get; set; }

        public SelectList UserList { get; set; }
        [BindProperty(SupportsGet = true)]
        public int UserId { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            var blog =  await _context.Blogs.FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            Blog = blog;
            // Danh sách người dùng
            UserList = new SelectList(await _context.Users.ToListAsync(), "Id", "FullName", Blog.UserId);
            // Gán UserId từ bài viết
            UserId = Blog.UserId;

            GetParentCategories();
            // Lấy danh sách danh mục con (nếu có)
            var parentCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == Blog.CategoryId);
            if (parentCategory != null && parentCategory.ParentCategoryId.HasValue)
            {
                ParentCategoryId = parentCategory.ParentCategoryId.Value; // Gán Parent ID
                SelectedCategoryId = Blog.CategoryId; // Gán giá trị của bài viết
                ChildCategories = await _context.Categories
                    .Where(c => c.ParentCategoryId == ParentCategoryId)
                    .ToListAsync();
            }
            else
            {
                ParentCategoryId = Blog.CategoryId; // Nếu không có danh mục con, giữ ID của danh mục cha
                SelectedCategoryId = null;
                ChildCategories = new List<Category>(); // Xóa danh mục con nếu không có
            }

            //ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");

            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Mail");
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
        /*    if (!ModelState.IsValid)
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
            }
            if (!ModelState.IsValid)
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

         /*   var existingUser = await _context.Users.FindAsync(Blog.UserId);
            if (existingUser == null)
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại.";
                return Page();
            }*/
            //Blog.UserId = Blog.UserId;

            _context.Attach(Blog).State = EntityState.Modified;
            SetAlert("Cập nhật Dữ liệu Thành công", "success");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(Blog.Id))
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

        private bool BlogExists(int id)
        {
          return (_context.Blogs?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public async Task<IActionResult> OnPostUploadAvatarAsync()
        {
            if (Upload == null || Upload.Length == 0)
                return BadRequest(new { success = false, error = "No file uploaded" });

            try
            {
                //var credential = GoogleCredential.FromFile(Path.Combine(
           //_environment.WebRootPath, "keys/firebase-key.json"));
                //var storage = StorageClient.Create();
                var storage = StorageClient.Create(FirebaseApp.DefaultInstance.Options.Credential);
                // Bucket name chính xác từ Firebase Console
                var bucket = "faise-1f797.firebasestorage.app";

                string uniqueFileName = $"{Guid.NewGuid()}_{Upload.FileName}";

                using (var stream = Upload.OpenReadStream())
                {
                    await storage.UploadObjectAsync(bucket, $"avatars/{uniqueFileName}", Upload.ContentType, stream);
                }

                var url = $"https://firebasestorage.googleapis.com/v0/b/{bucket}/o/{Uri.EscapeDataString($"avatars/{uniqueFileName}")}?alt=media";

                return new JsonResult(new { success = true, filePath = url });
            }
            catch (Exception ex)
            {
                // Trả về lỗi cụ thể từ Firebase
                return BadRequest(new { success = false, error = ex.ToString() });
            }

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

            /*if (Upload == null || Upload.Length == 0)
            {
                return BadRequest(new { uploaded = false, error = new { message = "No file uploaded" } });
            }
            // Get the current date and format it
            var currentDate = DateTime.Now;
            var year = currentDate.Year.ToString();
            var month = currentDate.Month.ToString().PadLeft(2, '0');
            var day = currentDate.Day.ToString().PadLeft(2, '0');

            // Create the directory string and ensure the directory exists
            var directoryPath = Path.Combine(_environment.WebRootPath, "uploads/images", year, month);
            Directory.CreateDirectory(directoryPath); // Creates all directories on the path if not exist

            // Modify the filename to include the date
            var uniqueFileName = $"{year}{month}{day}_{Path.GetFileName(Upload.FileName)}";
            var filePath = Path.Combine(directoryPath, uniqueFileName);



            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Upload.CopyToAsync(fileStream);
            }

            string fileUrl = $"/uploads/images/{uniqueFileName}";
            ViewData["Anh"] = uniqueFileName;

            return new JsonResult(new { uploaded = true, url = fileUrl });*/
        }

        public JsonResult OnGetChildCategories(int parentId)
        {
            var childCategories = _context.Categories
                                          .Where(c => c.ParentCategoryId == parentId)
                                          .Select(c => new {
                                              categoryId = c.CategoryId,
                                              categoryName = c.CategoryName
                                          })
                                          .ToList();

            return new JsonResult(childCategories);
        }

    }
}
