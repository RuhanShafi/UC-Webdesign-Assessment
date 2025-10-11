using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UC_Web_Assessment.Data; 
using UC_Web_Assessment.Models; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting; 
using System.IO;                   

namespace UC_Web_Assessment.Controllers
{
    [Authorize]
    public class AIImagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AIImagesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // INDEX (READ - All Users/Visitors)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var images = await _context.AIImage
                .Include(a => a.Likes)
                .ToListAsync();
            return View(images);
        }

        // CREATE (Member/Admin Access)
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AIImage aIImage)
        {
            if (ModelState.IsValid)
            {
                if (aIImage.ImageFile != null)
                {
                    // Validate file size (max 5MB)
                    if (aIImage.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
                        return View(aIImage);
                    }

                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(aIImage.ImageFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImageFile", "Only image files (jpg, png, gif, webp) are allowed.");
                        return View(aIImage);
                    }

                    try
                    {
                        aIImage.ImagePath = await SaveImageFile(aIImage.ImageFile);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("ImageFile", "Error uploading file: " + ex.Message);
                        return View(aIImage);
                    }
                }
                else
                {
                    ModelState.AddModelError("ImageFile", "Please select an image to upload.");
                    return View(aIImage);
                }

                aIImage.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                aIImage.CreatedDate = DateTime.Now;
                aIImage.LikeCount = 0;

                _context.Add(aIImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(aIImage);
        }

        // EDIT (Creator/Admin Access)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var aIImage = await _context.AIImage.FindAsync(id);
            if (aIImage == null) return NotFound();

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

            var originalImage = await _context.AIImage.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (originalImage == null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (originalImage.CreatorId != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            aIImage.CreatorId = originalImage.CreatorId;
            aIImage.CreatedDate = originalImage.CreatedDate;
            aIImage.ImagePath = originalImage.ImagePath;
            aIImage.LikeCount = originalImage.LikeCount;

            if (ModelState.IsValid)
            {
                try
                {
                    if (aIImage.ImageFile != null)
                    {
                        if (aIImage.ImageFile.Length > 5 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageFile", "File size must be less than 5MB.");
                            return View(aIImage);
                        }

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(aIImage.ImageFile.FileName).ToLower();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageFile", "Only image files are allowed.");
                            return View(aIImage);
                        }

                        if (!string.IsNullOrEmpty(originalImage.ImagePath))
                        {
                            string oldPath = Path.Combine(_hostEnvironment.WebRootPath, originalImage.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }

                        aIImage.ImagePath = await SaveImageFile(aIImage.ImageFile);
                    }
                    
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

        // DELETE (Admin Only)
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
                if (!string.IsNullOrEmpty(aIImage.ImagePath))
                {
                    string filePath = Path.Combine(_hostEnvironment.WebRootPath, aIImage.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.AIImage.Remove(aIImage);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DETAILS (Admin Only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var aIImage = await _context.AIImage
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aIImage == null) return NotFound();
            return View(aIImage);
        }

        // LIKE/UNLIKE ACTION
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var image = await _context.AIImage
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingLike = image.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
            {
                // Unlike
                _context.ImageLike.Remove(existingLike);
                image.LikeCount--;
            }
            else
            {
                // Like
                var like = new ImageLike { AIImageId = id, UserId = userId };
                _context.ImageLike.Add(like);
                image.LikeCount++;
            }

            await _context.SaveChangesAsync();

            return Json(new { likeCount = image.LikeCount, isLiked = existingLike == null });
        }

        // CHECK IF USER LIKED IMAGE (for AJAX)
        [AllowAnonymous]
        public async Task<IActionResult> IsImageLiked(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { isLiked = false });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isLiked = await _context.ImageLike
                .AnyAsync(l => l.AIImageId == id && l.UserId == userId);

            return Json(new { isLiked });
        }

        // IMAGE FILE UPLOAD HELPER
        private async Task<string> SaveImageFile(IFormFile file)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string uploadPath = Path.Combine(wwwRootPath, "images", "aiimages");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            string fileName = Path.GetFileNameWithoutExtension(file.FileName);
            string extension = Path.GetExtension(file.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;

            string pathForDb = "/images/aiimages/" + fileName;
            string fullPathOnServer = Path.Combine(uploadPath, fileName);

            using (var fileStream = new FileStream(fullPathOnServer, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return pathForDb;
        }
    }
}