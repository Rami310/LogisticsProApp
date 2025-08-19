using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using LogisticsPro.API.Data;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("🚀 Starting LogisticsPro API...");

// ✅ STEP 1: Add Entity Framework and MySQL FIRST
builder.Services.AddDbContext<LogisticsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Replace environment variable placeholder with actual value
    if (connectionString != null && connectionString.Contains("${MYSQL_PASSWORD}"))
    {
        var password = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
        
        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("❌ MYSQL_PASSWORD environment variable not set!");
            Console.WriteLine("💡 Please set it using one of these methods:");
            Console.WriteLine("   Windows: set MYSQL_PASSWORD=your_password");
            Console.WriteLine("   macOS/Linux: export MYSQL_PASSWORD=your_password");
            Console.WriteLine("   Or set it in your IDE's run configuration");
            throw new InvalidOperationException("MYSQL_PASSWORD environment variable is required");
        }
        
        connectionString = connectionString.Replace("${MYSQL_PASSWORD}", password);
        Console.WriteLine("✅ Database connection configured with environment variable");
    }
    else
    {
        Console.WriteLine("✅ Database connection string loaded from appsettings.json");
    }
    
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// ✅ STEP 2: Add HttpClient BEFORE controllers
builder.Services.AddHttpClient();
Console.WriteLine("✅ HttpClient service registered");

// ✅ STEP 3: Add CORS for frontend integration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ✅ STEP 4: Add controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

Console.WriteLine("✅ Controllers service registered");

// ✅ STEP 5: Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "LogisticsPro API", 
        Version = "v1",
        Description = "REST API for LogisticsPro Logistics Management System"
    });
});

Console.WriteLine("✅ All services registered successfully");

var app = builder.Build();

// ✅ STEP 6: Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LogisticsDbContext>();

    try
    {
        Console.WriteLine("🗄️  Testing database connection...");
        
        // Test connection
        await context.Database.CanConnectAsync();
        Console.WriteLine("✅ Database connection successful");
        
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ Database created/verified");
        
        // Seed data
        DatabaseSeeder.SeedData(context);
        Console.WriteLine("✅ Database seeded with sample data");
        
        // Show data counts
        var userCount = await context.Users.CountAsync();
        var productCount = await context.Products.CountAsync();
        var revenueCount = await context.CompanyRevenue.CountAsync();
        
        Console.WriteLine($"📊 Database ready: {userCount} users, {productCount} products, {revenueCount} revenue records");
        
        // ✅ CRITICAL: Seed revenue data
        await DatabaseSeeder.SeedRevenueDataAsync(context);
        
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database error: {ex.Message}");
        Console.WriteLine("💡 Check your MySQL server and environment variables!");
        Console.WriteLine("⚠️  API will start but database features won't work");
    }
}

// ✅ STEP 7: Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LogisticsPro API v1");
        c.RoutePrefix = string.Empty;
    });
    
    Console.WriteLine("📚 Swagger UI: https://localhost:7001");
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

// ✅ STEP 8: Map controllers
app.MapControllers();
Console.WriteLine("✅ Controllers mapped successfully");

// ✅ STEP 9: Health check endpoint with comprehensive testing
app.MapGet("/health", async (LogisticsDbContext context) => 
{
    try 
    {
        // Test database connectivity
        var canConnect = await context.Database.CanConnectAsync();
        var userCount = await context.Users.CountAsync();
        var revenueCount = await context.CompanyRevenue.CountAsync();
        var productCount = await context.Products.CountAsync();
        
        return Results.Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            database = new {
                connected = canConnect,
                userCount = userCount,
                revenueRecords = revenueCount,
                productCount = productCount
            },
            services = new {
                httpClientRegistered = true,
                controllersRegistered = true
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new { 
            status = "unhealthy", 
            timestamp = DateTime.UtcNow,
            error = ex.Message
        }, statusCode: 500);
    }
});

Console.WriteLine("🎯 API running: https://localhost:7001");
Console.WriteLine("❤️  Health check: https://localhost:7001/health");
Console.WriteLine("💰 Revenue API: https://localhost:7001/api/Revenue/current");
Console.WriteLine("📋 Product Requests: https://localhost:7001/api/ProductRequests");
Console.WriteLine("=" + new string('=', 50));

app.Run();