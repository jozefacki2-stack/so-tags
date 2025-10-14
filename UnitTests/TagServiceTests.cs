using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using TagsApi.Db;
using TagsApi.Services;

namespace UnitTests
{
    public class TagServiceTests
    {
        private class FakeClient : StackExchangeClient
        {
            private readonly (string name, int count)[] _data;

            public FakeClient((string name, int count)[] data)
                : base(new HttpClient(), new ConfigurationBuilder().Build())
            {
                _data = data;
            }

            public override async IAsyncEnumerable<TagResponseItem> GetPopularTagsAsync(int takeAtLeast)
            {
                foreach (var d in _data)
                    yield return new TagResponseItem(d.name, d.count);
                await Task.CompletedTask;
            }
        }

        [Test]
        public async Task Share_is_computed_to_100_percent_total()
        {
            var options = new DbContextOptionsBuilder<TagsDbContext>()
                .UseInMemoryDatabase("TestDb")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using var db = new TagsDbContext(options);

            var fakeClient = new FakeClient(new[]
            {
                ("csharp", 100),
                ("javascript", 300),
                ("java", 100)
            });

            var service = new TagService(db, fakeClient);

            await service.RefreshAsync();

            var totalShare = db.Tags.Sum(t => t.Share);

            Assert.That(totalShare, Is.InRange(99.999, 100.001));
        }

        [Test]
        public async Task RefreshAsync_overwrites_existing_tags()
        {
            var options = new DbContextOptionsBuilder<TagsDbContext>()
                .UseInMemoryDatabase("TestDb_Overwrite")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using var db = new TagsDbContext(options);

            var fakeClient1 = new FakeClient(new[] { ("csharp", 100), ("js", 200) });
            var service = new TagService(db, fakeClient1);
            await service.RefreshAsync();

            var firstCount = db.Tags.Count();

            var fakeClient2 = new FakeClient(new[] { ("python", 500) });
            var service2 = new TagService(db, fakeClient2);
            await service2.RefreshAsync();

            var tags = db.Tags.ToList();

            Assert.That(tags.Count, Is.EqualTo(1));
            Assert.That(tags.Single().Name, Is.EqualTo("python"));
        }

        [Test]
        public async Task RefreshAsync_handles_empty_data_gracefully()
        {
            var options = new DbContextOptionsBuilder<TagsDbContext>()
                .UseInMemoryDatabase("TestDb_Empty")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            using var db = new TagsDbContext(options);

            var fakeClient = new FakeClient(Array.Empty<(string name, int count)>());
            var service = new TagService(db, fakeClient);

            Assert.DoesNotThrowAsync(async () => await service.RefreshAsync());

            Assert.That(await db.Tags.CountAsync(), Is.EqualTo(0));
        }

    }
}
