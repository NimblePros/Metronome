using MetricTesting2.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetricTesting2.Pages
{
  public class UsersModel : PageModel
  {
    private readonly MetricTesting2.Data.ApplicationDbContext _context;

    public UsersModel(MetricTesting2.Data.ApplicationDbContext context)
    {
      _context = context;
    }

    public IList<AppRole> AppRole { get; set; } = default!;

    public async Task OnGetAsync()
    {
      AppRole = await _context.AppRole.ToListAsync();
    }
  }
}
