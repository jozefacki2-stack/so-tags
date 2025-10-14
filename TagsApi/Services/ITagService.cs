using TagsApi.Dtos;

namespace TagsApi.Services
{
    public interface ITagService
    {
        Task EnsureSeedAsync(int minTags);

        Task<PagedResult<TagDto>> GetPageAsync(int page, int pageSize, string sortBy, string order);

        Task<int> RefreshAsync();
    }
}
