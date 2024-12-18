using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using UserManager.Data;
using UserManager.Models;
using UserManager.Services;

var builder = WebApplication.CreateBuilder(args);



#region Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Registering ApplicationDbContext and defining connectionString
builder.Services.AddDbContext<ApplicationContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

#endregion


#region Registering And Configuring Identity


// defining our IdentityCore Service
builder.Services.AddIdentityCore<User>(options =>
{
    // password configuration
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;

    // for email confirmation
    options.SignIn.RequireConfirmedEmail = false;
}).AddRoles<IdentityRole>() // be able to add roles
.AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of RoleManager
.AddEntityFrameworkStores<ApplicationContext>() // providing our context
.AddSignInManager<SignInManager<User>>() // make use of Signin manager
.AddUserManager<UserManager<User>>() // make use of UserManager to create users
.AddDefaultTokenProviders(); // be able to create tokens for email confirmation



#endregion


#region Registering Needed Services

// be able to inject JWTService class inside our Controllers
builder.Services.AddScoped<JwtService>();

builder.Services.AddScoped<DataSeedingService>();

#endregion



#region Configuring Authentication And JwtBearer

// be able to authenticate users using JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // validate the token based on the key we have provided inside appSettings.development.json JWT:Key
            ValidateIssuerSigningKey = true,
            // the issuer SigningKey  based on JWT:Key
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"] ?? throw new InvalidOperationException())),
            // the issuer which in here is the api project url we are using
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            // validate the issuer (who ever is issuing the JWT)
            ValidateIssuer = true,
            // don't validate audience (angular side)
            ValidateAudience = false
        };
    });
#endregion



#region Shaping Error Messages
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .SelectMany(x => x.Value.Errors)
        .Select(x => x.ErrorMessage).ToArray();

        var toReturn = new
        {
            Errors = errors
        };

        return new BadRequestObjectResult(toReturn);
    };
});

#endregion

#region SetPolicy

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    opt.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));


});


#endregion


#region cors
builder.Services.AddCors();

#endregion

var app = builder.Build();

#region Configure Cors
app.UseCors(opt =>
{

    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("https://localhost:4200");
});
//


#endregion

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adding UseAuthentication into our pipeline and this should come before UseAuthorization
// Authentication verifies the identity of a user or service, and authorization determines their access rights.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#region DataSeeding Configuration
using var scope = app.Services.CreateScope();
try
{
    var contextSeedService = scope.ServiceProvider.GetService<DataSeedingService>();
    await contextSeedService.InitializeContextAsync();
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    logger.LogError(ex.Message, "Failed to initialize and seed the database");
}
#endregion
app.Run();
