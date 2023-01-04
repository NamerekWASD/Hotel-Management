using DAL;
using Entities.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UnitsOfWork;
using UnitsOfWork.Interfaces;
using WebApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<UsageDbContext>(options =>
    options.UseSqlServer(builder.Configuration
    .GetConnectionString("UsageConnection"),
                providerOptions =>
                { providerOptions.EnableRetryOnFailure(); })
    .UseLazyLoadingProxies());

builder.Services.AddIdentity<Customer, Role>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<UsageDbContext>()
    .AddDefaultUI()
    .AddTokenProvider<DataProtectorTokenProvider<Customer>>(TokenOptions.DefaultProvider);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options =>
               {
                   options.LoginPath = new PathString("/Account/Login");
               });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrator",
        authBuilder =>
        {
            authBuilder.RequireRole("Administrator");
        });
    options.AddPolicy("Manager",
        authBuilder =>
        {
            authBuilder.RequireRole("Administrator,Manager");
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
