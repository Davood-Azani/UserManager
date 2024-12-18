using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserManager.Models;

namespace UserManager.Data
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options) : IdentityDbContext<User>(options);
}
