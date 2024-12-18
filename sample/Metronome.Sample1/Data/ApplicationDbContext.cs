using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MetricTesting2.Data;

public class ApplicationDbContext : IdentityDbContext
{
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options)
  {
  }

  public DbSet<MetricTesting2.Data.AppRole> AppRole { get; set; } = default!;
}

public class AppUser : IdentityUser { }

public class AppRole : IdentityRole { }
