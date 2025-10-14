using Microsoft.EntityFrameworkCore;
using TagsApi.Db;
using TagsApi.Dtos;
using TagsApi.Models;

namespace TagsApi.Services
{
    public class TagService : ITagService
    {
        private readonly TagsDbContext _db;
        private readonly StackExchangeClient _client;
        public TagService(TagsDbContext db, StackExchangeClient client)
        { _db = db; _client = client; }


        public async Task EnsureSeedAsync(int minTags)
        {
            if (await _db.Tags.AnyAsync()) return;
            await RefreshInternalAsync(minTags);
        }


        public async Task<int> RefreshAsync() => await RefreshInternalAsync(1000);


        private async Task<int> RefreshInternalAsync(int minTags)
        {
            var list = new List<TagEntity>();
            await foreach (var t in _client.GetPopularTagsAsync(minTags))
            {
                list.Add(new TagEntity { Name = t.name, Count = t.count });
            }
            if (list.Count == 0) 
                return 0;

            double total = list.Sum(x => (double)x.Count);

            foreach (var t in list) 
                t.Share = Math.Round((t.Count / total) * 100.0, 6);

            using var trx = await _db.Database.BeginTransactionAsync();
            _db.Tags.RemoveRange(_db.Tags);
            await _db.SaveChangesAsync();
            await _db.Tags.AddRangeAsync(list);
            await _db.SaveChangesAsync();
            await trx.CommitAsync();

            return list.Count;
        }


        public async Task<PagedResult<TagDto>> GetPageAsync(int page, int pageSize, string sortBy, string order)
        {
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var q = _db.Tags.AsNoTracking();
            q = (sortBy, order.ToLower()) switch
            {
                ("name", "desc") => q.OrderByDescending(x => x.Name),
                ("name", _) => q.OrderBy(x => x.Name),
                ("share", "desc") => q.OrderByDescending(x => x.Share),
                ("share", _) => q.OrderBy(x => x.Share),
                _ => q.OrderBy(x => x.Name)
            };

            int total = await q.CountAsync();
            var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new TagDto(x.Name, x.Count, x.Share)).ToListAsync();

            return new PagedResult<TagDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                Items = items
            };
        }
    }
}
