namespace MetricTesting2.Pages;

public interface IPostsService
{
  Task<IEnumerable<Post>> GetPostsAsync();
}

