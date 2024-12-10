using Crowdfunding.Config;
using Crowdfunding.Data;
using Crowdfunding.Enums;
using Crowdfunding.Models;
using Crowdfunding.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

//configure ms sql server
builder.Services.AddDbContext<CrowdFundingDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CrowdfundingDB")));

//stripe
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

//set the stripe api key
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe")["SecretKey"];

//builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<CrowdFundingDBContext>();

//Add Identity Services
builder.Services.AddIdentity<User, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<CrowdFundingDBContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

//configure identity options
builder.Services.Configure<IdentityOptions>(options =>
{

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;

    options.SignIn.RequireConfirmedEmail = false; // Require email confirmation

});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole",
         policy => policy.RequireRole(UserRole.ADMIN.ToString()));

    options.AddPolicy("RequireProjectCreatorRole",
         policy => policy.RequireRole(UserRole.CREATOR.ToString()));

    options.AddPolicy("RequireBackerRole",
         policy => policy.RequireRole(UserRole.BACKER.ToString()));
});



// Register EmailSender service
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Initialize roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleInitializer.InitializeAsync(services);
}

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

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

