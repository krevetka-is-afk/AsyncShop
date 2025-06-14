using PaymentsService.Services;
using PaymentsService.Storage;
using Microsoft.EntityFrameworkCore;
using PaymentsService.Data;
var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("Configured OrdersService BaseUrl: " +
                  builder.Configuration["OrdersService:BaseUrl"]);

builder.Services.AddControllers();
// builder.Services.AddSingleton<InMemoryAccontStore>();
builder.Services.AddDbContext<PaymentsDbContext>(options => options.UseSqlite("Data Source=payments.db"));
builder.Services.AddHostedService<OrderConsumer>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddHttpClient<OrderStatusClient>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<RabbitMqStatusPublisher>();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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