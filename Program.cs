using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using DeepHumans.Data;
using DeepHumans.Models;
using DeepHumans.Services; // Your custom EmailSender
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using DeepHumans.Services; // AssistantService

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// 🔹 DATABASE CONFIGURATION (MySQL)
// ======================================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    )
);

// ======================================================
// 🔹 IDENTITY CONFIGURATION
// ======================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Disable email confirmation (register → login directly)
    options.SignIn.RequireConfirmedAccount = false;

    // Simplify password policy for dev/testing
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 0;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ======================================================
// 🔹 EMAIL SENDER REGISTRATION
// ======================================================
// Use your custom EmailSender for Microsoft’s IEmailSender interface
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, DeepHumans.Services.EmailSender>();

// ======================================================
// 🔹 SESSION SUPPORT
// ======================================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ======================================================
// 🔹 MVC & RAZOR
// ======================================================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ======================================================
// 🔹 AI Assistant Service (OpenAI)
// ======================================================
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IAssistantService, AssistantService>();

// ======================================================
// 🔹 BUILD THE APP
// ======================================================
var app = builder.Build();

// ======================================================
// 🔹 MIDDLEWARE PIPELINE
// ======================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Uncomment if using HTTPS
app.UseStaticFiles();

app.UseRouting();

// ✅ Order matters here
app.UseSession();          // 1️⃣ Enable session
app.UseAuthentication();   // 2️⃣ Enable authentication
app.UseAuthorization();    // 3️⃣ Enable authorization

// ======================================================
// 🔹 ROUTING
// ======================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// ======================================================
// 🔹 RUN THE APP
// ======================================================
app.Run();
