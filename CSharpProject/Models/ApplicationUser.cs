using Microsoft.AspNetCore.Identity;

namespace CSharpProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection <Order>? Orders { get; set; }
    }
}
