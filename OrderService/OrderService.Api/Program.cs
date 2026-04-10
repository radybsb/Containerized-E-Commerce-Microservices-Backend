using OrderService.Api.Data;
using OrderService.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

builder.Services.AddHttpClient<ICustomerClient, CustomerClient>(client => // allows us to call the Customer Service API via HTTP requests
{
    client.BaseAddress = new Uri("http://customerservice:8080/");
});

builder.Services.AddHttpClient<IProductClient, ProductClient>(client => // allows us to call the Product Service API via HTTP requests
{
    client.BaseAddress = new Uri("http://productservice:8080/");
});

builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
