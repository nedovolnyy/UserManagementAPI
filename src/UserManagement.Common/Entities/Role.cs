namespace UserManagement.Common.Entities;

public class Role : BaseEntity
{
    public Role()
    {
        Name = nameof(Roles.User);
        NormalizedName = nameof(Roles.User).Normalize();
    }

    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
}

public enum Roles
{
    User,
    Support,
    Admin,
    SuperAdmin,
}
