using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using DeepHumans.Data;
using DeepHumans.Models;
using DeepHumans.Services; // your EmailSender

var builder = WebApplication.CreateBuilder(args);

// ---------- DB ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Identity ----------
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// ---------- Email sender (YOUR implementation) ----------
builder.Services.AddTransient<DeepHumans.Services.IEmailSender, DeepHumans.Services.EmailSender>();

// ---------- Session (REQUIRED) ----------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ---------- MVC / Razor ----------

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // leave commented if you’re testing on plain HTTP
app.UseStaticFiles();

app.UseRouting();

// >>> VERY IMPORTANT ORDER <<<
app.UseSession();          // 1) session BEFORE auth
app.UseAuthentication();   // 2) authentication
app.UseAuthorization();    // 3) authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
