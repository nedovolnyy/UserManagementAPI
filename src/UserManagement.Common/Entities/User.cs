namespace UserManagement.Common.Entities;

public class User : BaseEntity
{
    required public string Name { get; set; }
    required public int Age { get; set; }
    required public string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string Roles { get; set; } = nameof(User);
}