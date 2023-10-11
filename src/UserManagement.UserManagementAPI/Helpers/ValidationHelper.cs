namespace UserManagement.UserManagementAPI.Helpers;

public class ValidationHelper
{
    protected ValidationHelper()
    {
    }

    public static async Task ValidateAsync(User user, IUserRepository userRepository)
    {
        if (string.IsNullOrEmpty(user.Name))
        {
            throw new ValidationException("The field 'Name' of User is not allowed to be empty!");
        }

        if (user.Age == default)
        {
            throw new ValidationException("The field 'Age' of User is not allowed to be null!");
        }

        if (user.Age < 1)
        {
            throw new ValidationException("The field 'Age' of User is not correct!");
        }

        if (string.IsNullOrEmpty(user.Email))
        {
            throw new ValidationException("The field 'Email' of User is not allowed to be empty!");
        }

        var chekingUser = await userRepository.GetUserByEmailAsync(user.Email);
        if (chekingUser is not null)
        {
            throw new ValidationException("An user exists with this email!");
        }
    }
}
