using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using UserManager.Models;
using UserManager.Utils;
using UserManager.Data;

namespace UserManager.Services
{
    public class DataSeedingService
    {

        private readonly ApplicationContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataSeedingService(ApplicationContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeContextAsync()
        {
            if (_context.Database.GetPendingMigrationsAsync().GetAwaiter().GetResult().Count() > 0)
            {
                // applies any pending migration into our database
                await _context.Database.MigrateAsync();
            }

            if (!_roleManager.Roles.Any())
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.AdminRole });
                await _roleManager.CreateAsync(new IdentityRole { Name = SD.UserRole });
               
            }

            if (!_userManager.Users.AnyAsync().GetAwaiter().GetResult())
            {
                var admin = new User
                {
                    FirstName = "admin",
                    LastName = "administrator",
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin, "123456");
                await _userManager.AddToRolesAsync(admin, new[] { SD.AdminRole, SD.UserRole });
                await _userManager.AddClaimsAsync(admin, new Claim[]
                {
                    new Claim(ClaimTypes.Email, admin.Email),
                    new Claim(ClaimTypes.Surname, admin.LastName)
                });

                var user = new User
                {
                    FirstName = "user",
                    LastName = "user",
                    UserName = "user@example.com",
                    Email = "user@example.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(user, "123456");
                await _userManager.AddToRoleAsync(user, SD.UserRole);
                await _userManager.AddClaimsAsync(user, new Claim[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Surname, user.LastName)
                });

               
            }
        }

    }
}
