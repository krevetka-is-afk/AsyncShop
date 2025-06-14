using PaymentsService.Services;
using PaymentsService.Storage;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Data;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
// Console.WriteLine("Configured OrdersService BaseUrl: " +
//                   builder.Configuration["OrdersService:BaseUrl"]);

builder.Services.AddControllers();
// builder.Services.AddSingleton<InMemoryAccontStore>();
builder.Services.AddDbContext<PaymentsDbContext>(options => options.UseSqlite("Data Source=payments.db"));
builder.Services.AddHostedService<OrderConsumer>();
builder.Services.AddScoped<AccountService>();
// builder.Services.AddHttpClient<OrderStatusClient>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<RabbitMqStatusPublisher>();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payments Service API", Version = "v1" });
});

var app = builder.Build();

// // Enable Swagger in all environments
// app.UseSwagger();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payments Service API V1");
//     c.RoutePrefix = "swagger";
// });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    Console.WriteLine($"[Middleware] {context.Request.Method} {context.Request.Path}");
    await next();
});
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();