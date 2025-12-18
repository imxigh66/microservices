using CatalogService.Data;
using CatalogService.Services;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
// Add services to the container.
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDb")));

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImageServiceClient, ImageServiceClient>();

// Настройка HttpClient для ImageService
builder.Services.AddHttpClient<IImageServiceClient, ImageServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ImageService:BaseUrl"]
        ?? "http://localhost:5001/");
    client.Timeout = TimeSpan.FromSeconds(30);
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var logger = services.GetRequiredService<ILogger<Program>>();

	try
	{
		var context = services.GetRequiredService<ProductDbContext>();

		logger.LogInformation("?? Применение миграций к базе данных...");
		context.Database.Migrate();
		logger.LogInformation("? Миграции успешно применены");
	}
	catch (Exception ex)
	{
		logger.LogError(ex, "? Ошибка при применении миграций");
		throw;
	}
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Configure the HTTP request pipeline.

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
