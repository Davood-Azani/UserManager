using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UserManager.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FirstName { get; set; } = default!;
        [Required]
        public string LastName { get; set; } = default!;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
