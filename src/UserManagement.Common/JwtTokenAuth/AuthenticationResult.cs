namespace UserManagement.Common.JwtTokenAuth;

public class AuthenticationResult
{
    public string? Token { get; set; }

    required public bool Result { get; set; }

    public User User { get; set; } = null!;

    public List<Roles>? Roles { get; set; }

    public List<string>? Errors { get; set; }
}
