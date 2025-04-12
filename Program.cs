using Integration_System.DAL;
using Integration_System.Middleware;
using Integration_System.Model;
using Integration_System.Services;

using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<EmployeeDAL>();
builder.Services.AddScoped<SalaryDAL>();
builder.Services.AddScoped<PositionDAL>();
builder.Services.AddScoped<DepartmentDAL>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Cho phép React truy cập
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379")); // hoặc redis:6379 nếu dùng Docker
builder.Services.AddSingleton<NotificationSalaryService>();
builder.Services.AddScoped<NotificationSalaryMDW>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("AllowSpecificOrigin");

app.Run();
