namespace UserManagement.UserManagementAPI.Models;

public class RegisterModel : User
{
    required public string Password { get; set; }
}