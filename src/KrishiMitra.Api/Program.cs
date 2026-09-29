using KrishiMitra.Core;
using KrishiMitra.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var solutionDirectory = new DirectoryInfo(builder.Environment.ContentRootPath);
while (solutionDirectory is not null && !File.Exists(Path.Combine(solutionDirectory.FullName, "KrishiMitra.sln"))) solutionDirectory = solutionDirectory.Parent;
var databaseRoot = solutionDirectory?.FullName ?? builder.Environment.ContentRootPath;
var databasePath = Path.Combine(databaseRoot, "App_Data", "krishimitra.db");
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
builder.Services.AddDbContext<FarmDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("FarmDb") ?? $"Data Source={databasePath}"));
builder.Services.AddScoped<IKnowledgeRetriever, KeywordKnowledgeRetriever>();
if (builder.Configuration["AI:Provider"]?.Equals("OpenAICompatible", StringComparison.OrdinalIgnoreCase) == true)
{
    builder.Services.AddHttpClient<OpenAiCompatibleAssistant>(client => client.Timeout = TimeSpan.FromSeconds(25));
    builder.Services.AddScoped<IAiAssistant>(sp => sp.GetRequiredService<OpenAiCompatibleAssistant>());
}
else builder.Services.AddScoped<IAiAssistant, MockAiAssistant>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FarmDbContext>();
    await db.Database.MigrateAsync();
}
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsJsonAsync(new { error = "An unexpected server error occurred." });
}));
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.Run();

public partial class Program { }
