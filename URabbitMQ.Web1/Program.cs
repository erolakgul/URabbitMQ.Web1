using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using URabbitMQ.Web1.Context;
using URabbitMQ.Web1.Services;
using URabbitMQ.Web1.Services.Pubs;
using URabbitMQ.Web1.Services.Subs.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region connection fac
var rabbitMQService = builder.Configuration.GetConnectionString("RabbitMQ");

builder.Services.AddSingleton(sp =>

    new ConnectionFactory()
    {
        Uri = new Uri(rabbitMQService),
        DispatchConsumersAsync = true  // ImageWaterMarkProcessBackgroundServices içinde execute => consumer AsyncEventingBasicConsumer asenkron methoduyla çaðýrýldýðý için 
    }
);

builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
#endregion

#region db in memory
builder.Services.AddDbContext<AppDbContext>(options =>
{
options.UseInMemoryDatabase(databaseName: "ProductDb");
}); 
#endregion

#region background service di a register edilir
builder.Services.AddHostedService<ImageWaterMarkProcessBackgroundServices>();
#endregion


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
