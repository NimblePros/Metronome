using MetricTesting2.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetricTesting2.Pages;

public class IndexModel : PageModel
{
  private readonly ILogger<IndexModel> _logger;
  private readonly IPostsService _postsService;
  private readonly ApplicationDbContext _dbContext;

  public IndexModel(ILogger<IndexModel> logger,
      IPostsService postsService,
      ApplicationDbContext dbContext)
  {
    _logger = logger;
    _postsService = postsService;
    _dbContext = dbContext;
  }

  public async Task OnGetAsync()
  {
    var result = await _postsService.GetPostsAsync();
    _logger.LogDebug("Posts fetched: {PostCount}", result.Count());

    var userCount = await _dbContext.Users.CountAsync();
    _logger.LogDebug("Users in database: {UserCount}", userCount);

    var roleCount = await _dbContext.Roles.CountAsync();
    _logger.LogDebug("Users in database: {roleCount}", roleCount);
  }
}


