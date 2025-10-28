using OrdersService.Services;
using OrdersService.Storage;
using OrdersService.Interfaces;
using Microsoft.EntityFrameworkCore;
using OrdersService.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite("Data Source=/app/data/orders.db"));

builder.Services.AddControllers();
builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddHostedService<OrderStatusConsumer>();
builder.Services.AddSingleton<IPaymentPublisher, RabbitMqPaymentPublisher>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders Service API", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();