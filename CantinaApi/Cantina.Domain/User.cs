using Microsoft.AspNetCore.Identity;

namespace Cantina.Domain
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
    }
}
