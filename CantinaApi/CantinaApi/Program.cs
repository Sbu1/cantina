using System.Text;
using System.Threading.RateLimiting;
using Cantina.Application.Interfaces;
using Cantina.Domain;
using Cantina.Infrastructure.Persistence;
using Cantina.Infrastructure.Persistence.DI;
using Cantina.Infrastructure.Persistence.Middleware;
using Cantina.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(option =>
{
    option.Password.RequireDigit = true;
    option.Password.RequiredLength = 8;
    option.Password.RequireNonAlphanumeric = true;
    option.Password.RequireUppercase = true;
    option.Password.RequireLowercase = true;

    // Account lockout settings (brute-force protection)
    option.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    option.Lockout.MaxFailedAccessAttempts = 5;
    option.Lockout.AllowedForNewUsers = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Lockout duration
    options.Lockout.MaxFailedAccessAttempts = 5; // Maximum failed attempts before lockout
    options.Lockout.AllowedForNewUsers = true;
});



builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,  
                Window = TimeSpan.FromMinutes(1), 
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 5  
            }));
    options.AddFixedWindowLimiter("loginLimiter", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;  // Allow 5 requests
        limiterOptions.Window = TimeSpan.FromMinutes(5); // Per 5 minutes
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;  // Extra 2 requests in queue
    });
});
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
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseDeveloperExceptionPage();
app.MapControllers().RequireRateLimiting("loginLimiter");

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
string adminEmail = "sbuddaz@gmail.com";
string adminPassword = "pas4Admin@123"; // Change this to a secure password

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