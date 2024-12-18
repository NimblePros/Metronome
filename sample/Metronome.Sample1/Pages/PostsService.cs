namespace MetricTesting2.Pages;
public class PostsService : IPostsService
{
  private readonly HttpClient _httpClient;

  public PostsService(HttpClient httpClient, ILogger<PostsService> logger)
  {
    _httpClient = httpClient;
    if (_httpClient.BaseAddress == null)
    {
      logger.LogWarning("Base Address is null");
      _httpClient.BaseAddress = new Uri("https://my-json-server.typicode.com/typicode/demo/");
    }
  }

  public async Task<IEnumerable<Post>> GetPostsAsync()
  {
    var url = "posts";

    return await _httpClient.GetFromJsonAsync<List<Post>>(url);
  }
}

