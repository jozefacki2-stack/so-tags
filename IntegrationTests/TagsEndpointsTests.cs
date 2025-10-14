using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using TagsApi.Dtos;

namespace IntegrationTests
{
    public class TagsEndpointsTests
    {
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void Setup()
        {
            var apiProjectPath = Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory, "../../../../TagsApi"));

            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseContentRoot(apiProjectPath);
                });
        }

        [Test]
        public async Task Get_returns_paged_data()
        {
            var client = _factory.CreateClient();

            var res = await client.GetFromJsonAsync<PagedResult<TagDto>>("/api/tags?page=1&pageSize=10");

            Assert.NotNull(res);
            Assert.That(res!.Items.Count, Is.LessThanOrEqualTo(10));
        }

        [TearDown]
        public void Cleanup()
        {
            _factory?.Dispose();
        }
    }
}