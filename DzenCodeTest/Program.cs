using DzenCode.BLL.Services;
using DzenCode.BLL.Services.Interfaces;
using DzenCode.Common.Helpers;
using DzenCode.DAL.Data;
using DzenCodeTest.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using MySqlConnector;

// Проверка аргументов командной строки для тестирования подключения
if (args.Length > 0 && args[0] == "--test-connection")
{
    try
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
            
        string connectionString = config.GetConnectionString("DefaultConnection") ?? "";
        using var connection = new MySqlConnection(connectionString);
        connection.Open();
        Console.WriteLine("Database connection successful!");
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database connection failed: {ex.Message}");
        Environment.Exit(1);
    }
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Environment.WebRootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");



builder.Services.AddDbContext<CommentsDBContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));


builder.Services.AddAutoMapper(typeof(CommentProfile));

builder.Services.AddControllers();


builder.Services.AddServiceExtensions();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DzenCodeTest API",
        Version = "v1",
        Description = "API TEST.)"
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", builder =>
    {
        builder.SetIsOriginAllowed(_ => true)
                   .AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .SetPreflightMaxAge(TimeSpan.FromDays(1d));
    });
});

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DzenCodeTest API v1");
    c.RoutePrefix = "swagger"; 
});

app.UseCors("AllowFrontend");

app.UseStaticFiles(); 

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
