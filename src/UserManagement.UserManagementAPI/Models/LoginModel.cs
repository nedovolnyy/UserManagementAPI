﻿namespace UserManagement.UserManagementAPI.Models;

public class LoginModel
{
    required public string Email { get; set; }
    required public string Password { get; set; }
}
