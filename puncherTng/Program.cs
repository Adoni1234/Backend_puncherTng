using Microsoft.EntityFrameworkCore;
using puncherTng.ContextDB;
using puncherTng.Initalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddCors(options => options.AddPolicy(name: "PuncherTngOrigins",
    policy =>
    {
        policy.WithOrigins("https://puncher-assitan-tng.vercel.app", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Authorization"); // Exponer el encabezado Authorization
    }));

builder.Services.AddScoped<IDInitializationDb, InitializationDb>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PuncherTngOrigins");

// Configure the application to handle OPTIONS requests
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "https://puncher-assitan-tng.vercel.app");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS, DELETE, PUT");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();

    try
    {
        var initializer = services.GetRequiredService<IDInitializationDb>();
        initializer.Initalize();
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "Error initializing the migrations");
    }
}
