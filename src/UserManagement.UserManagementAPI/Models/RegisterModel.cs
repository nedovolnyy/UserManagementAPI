namespace UserManagement.UserManagementAPI.Models;

public class RegisterModel
{
    required public string Name { get; set; }
    required public int Age { get; set; }
    required public string Email { get; set; }
    required public string Password { get; set; }
    public string? ConfirmPassword { get; set; }
}