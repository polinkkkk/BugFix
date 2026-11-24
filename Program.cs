using BugFix.Api.Data;
using BugFix.Api.Models;
using BugFix.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BugFix.Api.Options;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("BugFix");

builder.Services.AddDbContext<ContextDB>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.Configure<PasswordResetOptions>(builder.Configuration.GetSection("PasswordReset"));

builder.Services.AddScoped<IPasswordResetEmailSender, SmtpPasswordResetEmailSender>();

builder.Services
    .AddIdentity<AppUser, IdentityRole<Guid>>(options =>
    {
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ContextDB>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BugFix.Auth";

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.WebHost.UseUrls("http://127.0.0.1:5080");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();