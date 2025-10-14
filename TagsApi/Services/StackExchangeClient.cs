namespace TagsApi.Services
{
    public class StackExchangeClient
    {
        private readonly HttpClient _http;
        private readonly string? _apiKey;

        public StackExchangeClient(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _apiKey = configuration["StackExchange:Key"];
        }

        public record TagResponseItem(string name, int count);
        public record TagResponse(List<TagResponseItem> items, bool has_more, int? quota_remaining, int? backoff);


        public virtual async IAsyncEnumerable<TagResponseItem> GetPopularTagsAsync(int takeAtLeast)
        {
            int page = 1; int fetched = 0;
            while (true)
            {
                var url = $"tags?page={page}&pagesize=100&order=desc&sort=popular&site=stackoverflow&key={_apiKey}";

                var resp = await _http.GetFromJsonAsync<TagResponse>(url);
                if (resp == null || resp.items == null || resp.items.Count == 0) yield break;
                foreach (var it in resp.items)
                {
                    yield return it; fetched++;
                }
                if (resp.backoff.HasValue) await Task.Delay(TimeSpan.FromSeconds(resp.backoff.Value));
                if (!resp.has_more || fetched >= takeAtLeast) yield break;
                page++;
            }
        }
    }
}
