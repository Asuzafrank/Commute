using CommutePro.Application.Interfaces;
using CommutePro.Domain.Entities;
using CommutePro.Infrastructure.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Domain Entities
        public DbSet<Stop> Stops => Set<Stop>();
        public DbSet<Route> Routes => Set<Route>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<StopTime> StopTimes => Set<StopTime>();
        public DbSet<FavouriteStation> FavouriteStations => Set<FavouriteStation>();
        public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<CalendarDate> CalendarDates => Set<CalendarDate>();
        public DbSet<Agency> Agencies => Set<Agency>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Apply configurations
            builder.ApplyConfiguration(new StopConfiguration());
            builder.ApplyConfiguration(new RouteConfiguration());
            builder.ApplyConfiguration(new TripConfiguration());
            builder.ApplyConfiguration(new StopTimeConfiguration());
            builder.ApplyConfiguration(new FavouriteStationConfiguration());
            builder.ApplyConfiguration(new NotificationPreferencesConfiguration());
            builder.ApplyConfiguration(new ServiceConfiguration());
            builder.ApplyConfiguration(new CalendarDateConfiguration());
            builder.ApplyConfiguration(new AgencyConfiguration());

            // Configure PostgreSQL specific settings
            ConfigurePostgreSQL(builder);
        }
        private void ConfigurePostgreSQL(ModelBuilder builder)
        {
            // Use snake_case naming convention
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName()?.ToSnakeCase());

                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToSnakeCase());
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(key.GetName()?.ToSnakeCase());
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
                }
            }
        }
    }
    public static class StringExtensions
    {
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrEmpty(str)) return str;

            return string.Concat(str.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
        }
    }
}
