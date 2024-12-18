using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManager.DTOs.Account;
using UserManager.Models;
using UserManager.Services;
using UserManager.Utils;

namespace UserManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(JwtService jwtService,
            SignInManager<User>
                signInManager, UserManager<User> userManager)
        {
            _jwtService = jwtService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return Unauthorized("Invalid username or password");


            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.IsLockedOut)
            {
                return Unauthorized(
                    $"Your account has been locked. You should wait until {user.LockoutEnd} (UTC time) to be able to login");
            }

            if (!result.Succeeded)
            {
                // User has input an invalid password
                if (!user.UserName.Equals(SD.AdminUserName))
                {
                    // Incrementing AccessFailedCount of the AspNetUser by 1
                    await _userManager.AccessFailedAsync(user);
                }

                if (user.AccessFailedCount >= SD.MaximumLoginAttempts)
                {
                    // Lock the user for one day
                    await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddDays(1));
                    return Unauthorized(
                        $"Your account has been locked. You should wait until {user.LockoutEnd} (UTC time) to be able to login");
                }


                return Unauthorized("Invalid username or password");
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            return await Helpers.CreateApplicationUserDto(user, _jwtService);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await Helpers.CheckEmailExistsAsync(model.Email, _userManager))
            {
                return BadRequest($"An existing account is using {model.Email}, email address. Please try with another email address");
            }

            var userToAdd = new User
            {
                FirstName = model.FirstName.ToLower(),
                LastName = model.LastName.ToLower(),
                UserName = model.Email.ToLower(),
                Email = model.Email.ToLower(),
                EmailConfirmed = true
            };

            // creates a user inside our AspNetUsers table inside our database
            var result = await _userManager.CreateAsync(userToAdd, model.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            // Adding role
            await _userManager.AddToRoleAsync(userToAdd, SD.UserRole);

            return Ok(new JsonResult(new { title = "Account Created", message = "Your account has been created, please login your email address" }));


        }

        [Authorize]
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)!.Value);
            if (user is not null)
            {

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Unauthorized("You have been locked out");
                }
                return await Helpers.CreateApplicationUserDto(user, _jwtService);
            }

            return BadRequest("User or Password is Wrong!");
        }

    }
}
