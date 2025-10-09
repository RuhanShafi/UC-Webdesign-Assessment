using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UC_Web_Assessment.Data; 
using UC_Web_Assessment.Models; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Needed for User.FindFirstValue
using Microsoft.AspNetCore.Hosting; 
using System.IO;                   

namespace UC_Web_Assessment.Controllers
{
    // Ensure all AIImage pages require a login unless explicitly allowed.
    [Authorize]
    public class AIImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AIImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public AIImagesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // ----------------------------------------------------
        // 1. INDEX (READ - All Users/Visitors)
        // ----------------------------------------------------
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.AIImage.ToListAsync());
        }

        // ----------------------------------------------------
        // 2. CREATE (Member/Admin Access)
        // ----------------------------------------------------
        public IActionResult Create()
        {
            // [Authorize] on the class handles access
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AIImage aIImage) // Bind removed, we use the IFormFile property
        {
            if (ModelState.IsValid)
            {
                if (aIImage.ImageFile != null)
                {
                    // 1. Call the file saving helper
                    aIImage.ImagePath = await SaveImageFile(aIImage.ImageFile);
                }

                // 2. Set CreatorId and Date
                aIImage.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                aIImage.CreatedDate = DateTime.Now;

                // 3. Save model to database
                _context.Add(aIImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aIImage);
        }

        // ----------------------------------------------------
        // 3. EDIT (Creator/Admin Access - Custom Logic)
        // ----------------------------------------------------
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var aIImage = await _context.AIImage.FindAsync(id);
            if (aIImage == null) return NotFound();

            // AUTHORIZATION CHECK
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (aIImage.CreatorId != currentUserId && !isAdmin)
            {
                TempData["ErrorMessage"] = "Access Denied: You can only edit images you created.";
                return RedirectToAction(nameof(Index));
            }

            return View(aIImage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AIImage aIImage)
        {
            if (id != aIImage.Id)
            {
                return NotFound();
            }

            // 1. Load the original entity to get its non-form fields (CreatorId, CreatedDate, ImagePath)
            var originalImage = await _context.AIImage.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (originalImage == null)
            {
                return NotFound();
            }

            // 2. AUTHORIZATION CHECK: Use the CreatorId from the original database record
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (originalImage.CreatorId != currentUserId && !isAdmin)
            {
                // Deny access if not creator AND not admin
                return Forbid();
            }

            // 3. Update non-form fields on the posted model (aIImage) using original data
            aIImage.CreatorId = originalImage.CreatorId;
            aIImage.CreatedDate = originalImage.CreatedDate;
            aIImage.ImagePath = originalImage.ImagePath; // Start with the existing path

            if (ModelState.IsValid)
            {
                try
                {
                    // 4. Handle file upload if a new file was provided
                    if (aIImage.ImageFile != null)
                    {
                        // Optional: Delete old file first
                        if (!string.IsNullOrEmpty(originalImage.ImagePath))
                        {
                            string oldPath = Path.Combine(_hostEnvironment.WebRootPath, originalImage.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        // Save the new file and update the model's ImagePath
                        aIImage.ImagePath = await SaveImageFile(aIImage.ImageFile);
                    }
                    
                    // 5. Save changes to the database
                    _context.Update(aIImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.AIImage.Any(e => e.Id == aIImage.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(aIImage);
        }

        // ----------------------------------------------------
        // 4. DELETE (Admin Only)
        // ----------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var aIImage = await _context.AIImage.FirstOrDefaultAsync(m => m.Id == id);
            if (aIImage == null) return NotFound();
            return View(aIImage);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aIImage = await _context.AIImage.FindAsync(id);
            if (aIImage != null)
            {
                _context.AIImage.Remove(aIImage);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ----------------------------------------------------
        // 5. DETAILS (Admin Only)
        // ----------------------------------------------------
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var aIImage = await _context.AIImage.FirstOrDefaultAsync(m => m.Id == id);
            if (aIImage == null) return NotFound();
            return View(aIImage);
        }


        // Image upload
        private async Task<string> SaveImageFile(IFormFile file)
        {
            // 1. Define the path (wwwroot/images/aiimages)
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string uploadPath = Path.Combine(wwwRootPath, "images", "aiimages");

            // Ensure the directory exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // 2. Create a unique file name to avoid any duplicate using datetime | Let's avoid any exception handeling in a language you're learning for a project
            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

            // The path we'll save in the DB (relative to wwwroot)
            string pathForDb = "/images/aiimages/" + fileName;
            string fullPathOnServer = Path.Combine(uploadPath, fileName);

            // 3. Save the file
            using (var fileStream = new FileStream(fullPathOnServer, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return pathForDb;
        }
    }
    
}