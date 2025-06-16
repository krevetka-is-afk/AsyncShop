using OrdersService.Services;
using OrdersService.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrdersService.Application.Services;
using OrdersService.Application.UseCases;
using OrdersService.Domain.Interfaces;
using OrdersService.Infrastructure.Data;
using OrdersService.Infrastructure.Messaging;
using OrdersService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite("Data Source=/app/data/orders.db"));

builder.Services.AddControllers();
builder.Services.AddHostedService<OutboxProcessor>();
builder.Services.AddHostedService<OrderStatusConsumer>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<CreateOrderUseCase>();
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