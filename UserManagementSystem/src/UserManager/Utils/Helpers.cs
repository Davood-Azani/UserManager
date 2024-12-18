using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManager.DTOs.Account;
using UserManager.Models;
using UserManager.Services;

namespace UserManager.Utils
{
    public static class Helpers
    {
        public static async Task<UserDto> CreateApplicationUserDto(User user , JwtService jwtService )
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                JWT = await jwtService.CreateJwt(user),
            };
        }
        public static async Task<bool> CheckEmailExistsAsync(string email, UserManager<User> userManager)
        {
            return await userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }
    }
}
