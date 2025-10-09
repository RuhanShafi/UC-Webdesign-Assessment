using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UC_Web_Assessment.Data; 
using UC_Web_Assessment.Models; 
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Needed for User.FindFirstValue

namespace UC_Web_Assessment.Controllers
{
    // Ensure all AIImage pages require a login unless explicitly allowed.
    [Authorize] 
    public class AIImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AIImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ----------------------------------------------------
        // 1. INDEX (READ - All Users/Visitors)
        // Visitor access: The PDF implies this gallery is viewable by all.
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
        public async Task<IActionResult> Create([Bind("Title,Description,ImagePath")] AIImage aIImage)
        {
            if (ModelState.IsValid)
            {
                // CRITICAL: Save the CreatorId
                aIImage.CreatorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                aIImage.CreatedDate = DateTime.Now;

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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ImagePath,CreatedDate,CreatorId")] AIImage aIImage)
        {
            if (id != aIImage.Id) return NotFound();

            // Re-run the authorization check before saving the changes
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");
            
            if (aIImage.CreatorId != currentUserId && !isAdmin)
            {
                 // This handles a malicious user trying to POST unauthorized changes
                return Forbid(); 
            }

            if (ModelState.IsValid)
            {
                try
                {
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
    }
}