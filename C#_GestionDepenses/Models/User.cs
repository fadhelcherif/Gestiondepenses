using Microsoft.AspNetCore.Identity;

namespace C__GestionDepenses.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; } // "Responsable" or "User"
    }
}

