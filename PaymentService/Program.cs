using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Kafka;
using PaymentService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", p =>
		p.AllowAnyOrigin()
		 .AllowAnyMethod()
		 .AllowAnyHeader());
});

// Db
builder.Services.AddDbContext<PaymentDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("PaymentDb")));

// Services
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<PaymentService.Services.PaymentService>();
builder.Services.AddSingleton<PaymentProducer>();

// Kafka consumer
builder.Services.AddHostedService<OrderCreatedConsumer>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var logger = services.GetRequiredService<ILogger<Program>>();

	try
	{
		var context = services.GetRequiredService<PaymentDbContext>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
