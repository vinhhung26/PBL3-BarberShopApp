using BarberShopApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<BarberShopAppContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. MVC + API Controllers + JSON camelCase
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Trả JSON với camelCase: billId, fullName, ...
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

// 3. Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// 4. Routes
// API Route (phải khai báo TRƯỚC route MVC)
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Index}/{id?}");

// MVC Route mặc định → mở trang đăng nhập
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapStaticAssets();

app.Run();