
using CommutePro.Api.Middleware;
using CommutePro.Application;
using CommutePro.Application.Interfaces.ML;
using CommutePro.Domain.Entities;
using CommutePro.Infrastructure;
using CommutePro.Infrastructure.Data;
using CommutePro.Infrastructure.Hubs;
using CommutePro.Infrastructure.ML.Training;
using CommutePro.Infrastructure.Seeder;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Serilog;

using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)

    // Enrichers
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithCorrelationIdHeader("X-Correlation-ID")
    .Enrich.WithProperty("Application", "CommutePro")

    // Console logging (Development)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({CorrelationId}) {Message:lj}{NewLine}{Exception}")

    // File logging - All logs
    .WriteTo.File(
        "logs/commutepro-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) [{Application}] {Message:lj}{NewLine}{Exception}")

    // File logging - Errors only (separate file for easy debugging)
    .WriteTo.File(
        "logs/errors-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({CorrelationId}) [{Application}] {Message:lj}{NewLine}{Exception}{NewLine}")

    .CreateLogger();

try
{
    Log.Information("Starting up CommutePro API");
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
    builder.Host.UseSerilog();
    builder.Services.AddSignalR();
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CommutePro API",
            Version = "v1",
            Description = "Real-time train tracking and alerts API",
            Contact = new OpenApiContact
            {
                Name = "CommutePro Support",
                Email = "support@commutepro.com"
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token. Example: eyJhbGciOiJIUzI1NiIs..."
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


;
    });
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

    builder.Services.AddMemoryCache();


    var app = builder.Build();

    if (args.Length > 0 && args[0] == "train-model")
    {
        ModelTrainer.TrainAndSaveModel();
        return;
    }

    _ = Task.Run(async () =>
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var mlService = scope.ServiceProvider.GetRequiredService<IDelayPredictionService>();
            await mlService.TrainModelAsync();
            Log.Information("ML model trained successfully");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ML model training failed - predictions will use defaults");
        }
    });
    // Initialize database on startup
    using (var scope = app.Services.CreateScope())
    {
        await DatabaseInitializer.InitializeAsync(scope.ServiceProvider);
    }
    app.UseCors("AllowFrontend");
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms | User: {UserName}";

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name ?? "anonymous");
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        };

        // Don't log health checks or static files
        options.GetLevel = (httpContext, elapsed, ex) =>
        {
            if (httpContext.Request.Path.StartsWithSegments("/health") ||
                httpContext.Request.Path.StartsWithSegments("/static"))
                return LogEventLevel.Debug;

            if (ex != null)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode >= 500)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode >= 400)
                return LogEventLevel.Warning;

            return LogEventLevel.Information;
        };
    });


    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseMiddleware<GlobalExceptionHandler>();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
       
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
            c.RoutePrefix = "swagger"; // Set to empty string to serve at root
        });
       
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<RealtimeHub>("/realtimeHub");
    app.MapFallbackToFile("/index.html");
    

    app.Run();

}
catch (Exception ex)
{

    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


