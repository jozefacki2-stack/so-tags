using Microsoft.EntityFrameworkCore;
using TagsApi.Db;
using TagsApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var conn = "Data Source=" + Path.Combine(AppContext.BaseDirectory, "tags.db");
builder.Services.AddDbContext<TagsDbContext>(o => o.UseSqlite(conn));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<StackExchangeClient>(client =>
{
    client.BaseAddress = new Uri("https://api.stackexchange.com/2.3/");
    client.DefaultRequestHeaders.Add("User-Agent", "StackTagsApp/1.0");
});

builder.Services.AddScoped<ITagService, TagService>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin()));

var app = builder.Build();

string? dataDir = null;
{
    const string key = "Data Source=";
    var idx = conn.IndexOf(key, StringComparison.OrdinalIgnoreCase);
    if (idx >= 0)
    {
        var rest = conn[(idx + key.Length)..];
        var semi = rest.IndexOf(';');
        var dataPath = semi >= 0 ? rest[..semi] : rest;
        dataDir = Path.GetDirectoryName(dataPath);
    }
    if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
        Directory.CreateDirectory(dataDir!);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TagsDbContext>();
    db.Database.EnsureCreated();
    var svc = scope.ServiceProvider.GetRequiredService<ITagService>();
    await svc.EnsureSeedAsync(minTags: 1000);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapControllers();

app.Run();

public partial class Program { }
