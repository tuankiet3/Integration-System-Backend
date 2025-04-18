using Integration_System.DAL;
using Integration_System.Middleware;
using Integration_System.Services;
using Integration_System.Settings;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Integration_System.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Integration_System.Constants;


var builder = WebApplication.CreateBuilder(args);

// Controller and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Data Access Layer
builder.Services.AddScoped<EmployeeDAL>();
builder.Services.AddScoped<SalaryDAL>();
builder.Services.AddScoped<PositionDAL>();
builder.Services.AddScoped<DepartmentDAL>();
builder.Services.AddScoped<AttendanceDAL>();

// CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Redis and Notification Service
try
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = ConfigurationOptions.Parse("localhost:6379", true);
        configuration.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(configuration);
    });
    builder.Services.AddSingleton<NotificationSalaryService>();
}
catch (RedisConnectionException ex)
{
    
    Console.WriteLine($"FATAL: Could not connect to Redis. {ex.Message}");
    
}

// Middleware
builder.Services.AddScoped<NotificationSalaryMDW>();
// Google Auth and Email Service
builder.Services.Configure<GoogleAuthSettings>(builder.Configuration.GetSection("GoogleAuth"));
builder.Services.AddSingleton<IGoogleAuthService, GoogleAuthService>(); 
builder.Services.AddTransient<IGmailService, GmailService>(); 

// DbContext for Authentication (IntegrationAuthDB)
var authConnectionString = builder.Configuration.GetConnectionString("AuthDbConnection");
if (string.IsNullOrEmpty(authConnectionString))
{
    throw new InvalidOperationException("Connection string 'AuthDbConnection' not found.");
}
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(authConnectionString));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

// Jwt Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false, 
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.")))
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.UseCors("AllowSpecificOrigin");

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// --- Seed Roles vào Database ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = {
            UserRoles.Admin,
            UserRoles.Hr,
            UserRoles.PayrollManagement,
            UserRoles.Employee
        };

        logger.LogInformation("Seeding roles...");
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                }
                else
                {
                    // Ghi log lỗi chi tiết từ IdentityError
                    foreach (var error in roleResult.Errors)
                    {
                        logger.LogError("Error creating role '{RoleName}': {ErrorCode} - {ErrorDescription}", roleName, error.Code, error.Description);
                    }
                }
            }
            else
            {
                logger.LogInformation("Role '{RoleName}' already exists. Skipping creation.", roleName);
            }
        }
        logger.LogInformation("Role seeding finished.");
    }
    catch (Exception ex)
    {
        // Catch các lỗi tiềm ẩn khác (ví dụ: lỗi kết nối db khi seeding)
        logger.LogError(ex, "An error occurred while seeding the roles.");
    }
}
app.Run();