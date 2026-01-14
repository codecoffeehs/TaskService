// using CatalogService.Configurations;
// using CatalogService.Context;
// using CatalogService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Auth.Shared;
using TaskService.Utils;
using MassTransit;
using TaskService.Context;
using TaskService.Services;
// using CatalogService.Consumers;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedJwtAuth(builder.Configuration);

builder.Services.AddControllers();

// builder.Services.AddMassTransit(x =>
// {
//     
//     x.SetEndpointNameFormatter(
//         new KebabCaseEndpointNameFormatter("task", false)
//     );
//
//     x.UsingRabbitMq((context, cfg) =>
//     {
//         var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
//         var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest";
//         var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest";
//         cfg.Host(host, "/", h =>
//         {
//             h.Username(user);
//             h.Password(pass);
//         });
//         
//         cfg.ConfigureEndpoints(context);
//
//     });
// });


// Add Swagger/OpenAPI with Bearer authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Service API",
        Version = "v1"
    });

    // Add Bearer token authorization
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AllTasksService>();
builder.Services.AddScoped<TaskCategoryService>();
// ✅ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        var origins = new List<string>
        {
            "https://admin.grahak24.com",
            "https://grahak24.com",
            "https://blogs.grahak24.com"
        };

        // Add development origins only in Development environment
        if (builder.Environment.IsDevelopment())
        {
            origins.Add("http://localhost:3000");
            origins.Add("http://localhost:3001");
            origins.Add("http://localhost:5173");
        }

        policy.WithOrigins([.. origins])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});



// ✅ DTO Validation Middleware
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        return new BadRequestObjectResult(new
        {
            message = "Validation failed.",
            errors
        });
    };
});



var app = builder.Build();

if (args.Contains("--migrate"))
{
    Console.WriteLine("Starting Migrations");
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("✓ Migrations applied successfully");
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Migration failed: {ex.Message}");
        Environment.Exit(1);
    }
}

if (builder.Environment.IsProduction())
{
    app.Urls.Add("http://0.0.0.0:5006");
}


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API v1");
    });
}

app.UseCors("AllowFrontend");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();