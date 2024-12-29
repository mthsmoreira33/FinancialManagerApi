using FinancialManagerApi.Data;
using FinancialManagerApi.Models;
using FinancialManagerApi.Routes;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var cookieSettings = builder.Configuration.GetSection("CookieSettings").Get<CookieSettings>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        if (cookieSettings != null)
        {
            options.Cookie.HttpOnly = cookieSettings.HttpOnly;
            options.Cookie.SecurePolicy = cookieSettings.Secure ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
            options.Cookie.SameSite = Enum.TryParse(cookieSettings.SameSite, out SameSiteMode sameSiteMode)
                ? sameSiteMode
                : SameSiteMode.Lax;
            options.Cookie.Name = cookieSettings.Name;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(cookieSettings.ExpirationMinutes);
        }

        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAuthenticatedUser", policy => policy.RequireAuthenticatedUser());

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapUserRoutes();
app.MapTransactionRoutes();

app.Run();