using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using DeepHumans.Data;
using DeepHumans.Models;
using DeepHumans.Services; // Your custom EmailSender
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using DeepHumans.Services; // AssistantService

var builder = WebApplication.CreateBuilder(args);

//  DATABASE CONFIGURATION 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))
    )
);

//  IDENTITY CONFIGURATION 
/*
This enables ASP.NET Identity, which handles:
User registration
Login / logout
Password hashing
Roles and permissions
*/
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Strong password policy
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 10;
    options.Password.RequiredUniqueChars = 4;

    // Account lockout after failed attempts
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();


/* SECURE COOKIE CONFIGURATION
   abdullah note : this will not work on localhost without https or real web app dont change it
*/
var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = isDevelopment ? "DeepHumans.Auth" : "__Host-DeepHumans.Auth";
    
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

//  ANTI-FORGERY TOKEN PROTECTION
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = isDevelopment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = isDevelopment ? "DeepHumans.AntiForgery" : "__Host-DeepHumans.AntiForgery";
});

//  EMAIL SENDER REGISTRATION
builder.Services.AddTransient<IEmailSender, EmailSender>();

//  SESSION SUPPORT
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//  MVC & RAZOR

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//  AI Assistant Service (OpenAI)
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IAssistantService, AssistantService>();

//  BUILD THE APP
var app = builder.Build();

//  MIDDLEWARE PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();          
app.UseAuthentication();   
app.UseAuthorization();    

// 🔹 ROUTING

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// 🔹 RUN THE APP
app.Run();
