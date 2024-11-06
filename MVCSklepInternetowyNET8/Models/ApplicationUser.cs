using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public Customer Customer { get; set; }
}
