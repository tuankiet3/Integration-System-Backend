    using Integration_System.DAL;
    using Integration_System.Middleware;
    using Integration_System.Model;
    using Integration_System.Services;
    using Microsoft.Extensions.Options; // Thêm using này
    using StackExchange.Redis;

    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Đăng ký DALs
    builder.Services.AddScoped<EmployeeDAL>();
    builder.Services.AddScoped<SalaryDAL>();
    builder.Services.AddScoped<PositionDAL>();
    builder.Services.AddScoped<DepartmentDAL>();

    // Đăng ký CORS
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

    // Đăng ký Redis và Notification Service
    builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
    builder.Services.AddSingleton<NotificationSalaryService>();

    // Đăng ký Middleware
    builder.Services.AddScoped<NotificationSalaryMDW>();

    
    // Đăng ký Email Service

    var app = builder.Build();

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