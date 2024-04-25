using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineSchool.Application.EncryptionServiceInterface;
using OnlineSchool.Domain.Contexts;
using OnlineSchool.Domain.Entities;
using OnlineSchool.Infrastructure.EncryptionServiceImplementation;
using OnlineSchool.Presentation.Hubs;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR();
builder.Services.AddTransient<IEncryptionService,EncryptionService>();
var serviceProvider = builder.Services.BuildServiceProvider();
var logger = serviceProvider.GetService<ILogger<ApplicationLogger>>();
//Add Singleton if you want to use Generic class logger in place of ILogger<T>
builder.Services.AddSingleton(typeof(ILogger), logger);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OnlineSchoolDb")));
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    //previous code removed for clarity reasons
    opt.Lockout.AllowedForNewUsers = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    opt.Lockout.MaxFailedAccessAttempts = 3;

}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/Login"; // Specify the route to your custom login page.
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");
app.MapHub<StreamingHub>("/streaminghub");
app.MapHub<PrivateHub>("/privateHub");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Dashboard}/{id?}");

app.Run();

public class ApplicationLogger{}
