using C__GestionDepenses.Data;
using C__GestionDepenses.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie settings
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Seed roles and a default Responsable user on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndResponsable(services);
}

app.Run();

// Method to create roles and a default Responsable user
async Task SeedRolesAndResponsable(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

    string[] roles = { "Responsable", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
            if (!roleResult.Succeeded)
            {
                Console.WriteLine($"Failed to create role {role}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    // Create a default Responsable user if not exists
    string responsableEmail = "responsable@admin.com";
    string responsablePassword = "Responsable123!";
    var responsableUser = await userManager.FindByEmailAsync(responsableEmail);
    if (responsableUser == null)
    {
        responsableUser = new User { UserName = responsableEmail, Email = responsableEmail, FullName = "Responsable Admin" };
        var result = await userManager.CreateAsync(responsableUser, responsablePassword);
        if (result.Succeeded)
        {
            var addRoleResult = await userManager.AddToRoleAsync(responsableUser, "Responsable");
            if (!addRoleResult.Succeeded)
            {
                Console.WriteLine($"Failed to add Responsable role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine($"Failed to create Responsable user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
    else
    {
        // Ensure user is in Responsable role
        if (!await userManager.IsInRoleAsync(responsableUser, "Responsable"))
        {
            var addRoleResult = await userManager.AddToRoleAsync(responsableUser, "Responsable");
            if (!addRoleResult.Succeeded)
            {
                Console.WriteLine($"Failed to add Responsable role to existing user: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
            }
        }
    }
}
