using Microsoft.AspNetCore.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ICollection<ApplicationUserRole> Users { get; set; } = [];
}