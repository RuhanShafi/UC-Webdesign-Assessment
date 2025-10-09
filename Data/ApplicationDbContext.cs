using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UC_Web_Assessment.Models;

namespace UC_Web_Assessment.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UC_Web_Assessment.Models.AIImage> AIImage { get; set; } 
}
