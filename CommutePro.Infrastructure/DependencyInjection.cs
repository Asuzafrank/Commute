using CommutePro.Application.Interfaces;
using CommutePro.Application.Interfaces.ML;
using CommutePro.Application.Interfaces.Repositories;
using CommutePro.Application.Interfaces.Services;
using CommutePro.Infrastructure.BackgroundServices;
using CommutePro.Infrastructure.Data;
using CommutePro.Infrastructure.Repositories;
using CommutePro.Infrastructure.Seeder;
using CommutePro.Infrastructure.Services;
using CommutePro.Infrastructure.Services.Cache;
using CommutePro.Infrastructure.Services.Gtfs;
using CommutePro.Infrastructure.Services.ML;
using CommutePro.Infrastructure.Services.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add PostgreSQL DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            // Register IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            
            services.AddScoped<IStationRepository, StationRepository>();
            services.AddScoped<IRouteRepository, RouteRepository>();
            services.AddScoped<ITripRepository, TripRepository>();
            services.AddScoped<IStopTimeRepository, StopTimeRepository>();
            services.AddScoped<IFavouriteRepository, FavouriteRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IAgencyRepository, AgencyRepository>();          
            services.AddScoped<ICalendarDateRepository, CalendarDateRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IRealtimeHubService, RealtimeHubService>();
            services.AddScoped<IDelayDetectionService, DelayDetectionService>();
            services.AddSingleton<IDelayPredictionService, DelayPredictionService>();

            // Register GTFS importer

            services.AddScoped<GtfsStaticImporter>();
            services.AddHttpClient<IGtfsRealtimeClient, MbtaRealtimeClient>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "CommutePro/1.0");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });
            // cache service(singleton so all handlers share same cache)
            services.AddSingleton<IRealtimeCacheService, RealtimeCacheService>();

            // background polling service
            services.AddHostedService<GtfsRealtimePollingService>();
            
            // Add JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")))
                };

                // For SignalR token in query string
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/realtimeHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            
            
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
