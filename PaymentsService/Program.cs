using PaymentsService.Services;
using PaymentsService.Storage;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// builder.Services.AddSingleton<InMemoryAccontStore>();
builder.Services.AddDbContext<PaymentsDbContext>(options => options.UseSqlite("Data Source=payments.db"));
builder.Services.AddHostedService<OrderConsumer>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<RabbitMqStatusPublisher>();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payments Service API", Version = "v1" });
});

var app = builder.Build();

// Apply migrations at startup
try
{
    Console.WriteLine("Applying database migrations...");
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.Migrate();
    Console.WriteLine("Database migrations applied successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error applying migrations: {ex}");
    throw;
}

// Enable Swagger in all environments
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments Service API V1");
    c.RoutePrefix = "swagger";
});

app.Use(async (context, next) =>
{
    Console.WriteLine($"[Middleware] {context.Request.Method} {context.Request.Path}");
    await next();
});
app.UseAuthorization();

app.MapControllers();

app.Run();