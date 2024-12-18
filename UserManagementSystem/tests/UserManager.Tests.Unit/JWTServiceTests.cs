using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using UserManager.Models;
using UserManager.Services;
using Xunit.Abstractions;

namespace UserManager.Tests.Unit
{
    public class JWTServiceTests
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly JwtService _jwtService;

        public JWTServiceTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            // Mock IConfiguration
            _config = Substitute.For<IConfiguration>();
            _config["JWT:Key"].Returns("3g3oKD6XHUZzzXQbfH2BXwpS146Q3KF9-86ruy2CK0YY4S3fqD3qgvvdpKmRCd5xi");
            _config["JWT:ExpiresInDays"].Returns("30");
            _config["JWT:Issuer"].Returns("http://localhost:5000");

            // Mock UserManager
            _userManager = Substitute.For<UserManager<User>>(
                Substitute.For<IUserStore<User>>(),
                null, null, null, null, null, null, null, null);

            // Create JwtService instance
            _jwtService = new JwtService(_config, _userManager);
        }
        [Fact]
        public async Task CreateJwt_ShouldReturnTokenَAndMatchEmailWithUserEmail_WhenTakesUser()
        {
            // Arrange
            var user = new User
            {
                Id = "123",
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            var roles = new List<string> { "Admin", "User" };
            _userManager.GetRolesAsync(user).Returns(roles);

            // Act
            var token = await _jwtService.CreateJwt(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Extract claims from the token
            var claims = jwtToken.Claims.ToList();
            _outputHelper.WriteLine(token); // print token
            var email = claims.SingleOrDefault(a => a.Type == "email")?.Value;

            // Assert

            #region Comment
            //foreach (var claim in claims)
            //{
            //    _outputHelper.WriteLine($"{claim.Type}: {claim.Value}"); // print claims
            //}

            #endregion

            token.Should().NotBeNull();
            email.Should().NotBeNull();
            email.Should().Be(user.Email);


        }
       


     
    }
}

     
