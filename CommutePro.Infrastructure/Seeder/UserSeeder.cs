using CommutePro.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommutePro.Infrastructure.Seeder
{
    public class UserSeeder
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(UserManager<ApplicationUser> userManager, ILogger<UserSeeder> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedUsersAsync()
        {
            var users = new List<(string email, string userName, string password)>
        {
            ("githubrian331@gmail.com", "brian", "Password123!"),
            ("carolinegatwiri771@gmail.com", "caroline", "Password123!"),
            ("hildanyago@gmail.com", "esther", "Password123!"),
            ("asuzafrank12@gmail.com", "frankline", "Password123!")
        };

            foreach (var (email, userName, password) in users)
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    _logger.LogInformation("User {Email} already exists, skipping", email);
                    continue;
                }

                var user = ApplicationUser.Create(email, userName);
                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Initialize notification preferences
                    user.InitializeNotificationPreferences();
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("Created user: {Email} with username: {UserName}", email, userName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create user {Email}: {Errors}", email, errors);
                }
            }
        }
    }
}
