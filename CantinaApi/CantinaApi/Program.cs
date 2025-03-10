using System.Text;
using Cantina.Application.Interfaces;
using Cantina.Domain;
using Cantina.Infrastructure.Persistence;
using Cantina.Infrastructure.Persistence.DI;
using Cantina.Infrastructure.Persistence.Middleware;
using Cantina.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScoped<IDishRepository, DishRepository>();
builder.Services.AddScoped<IDrinkRepository, DrinkRepository>();
//builder.Services.AddInfrastructure();


var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]); //needs to be moved from here 

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdminAsync(services);
}
    app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseDeveloperExceptionPage();
app.MapControllers();

app.Run();

static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

// Ensure the Admin role exists
string adminRole = "Admin";
if (!await roleManager.RoleExistsAsync(adminRole))
{
    await roleManager.CreateAsync(new IdentityRole(adminRole));
}

// Create Admin User
string adminEmail = "admin@example.com";
string adminPassword = "Admin@123"; // Change this to a secure password

var adminUser = await userManager.FindByEmailAsync(adminEmail);
if (adminUser == null)
{
    adminUser = new User
    {
        UserName = adminEmail,
        Email = adminEmail,
        FullName = "System Administrator"
    };
    var result = await userManager.CreateAsync(adminUser, adminPassword);
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }
}
}