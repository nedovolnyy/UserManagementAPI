namespace UserManagement.Common.DI;

public interface IUserRepository
{
    /// <summary>
    /// Base method for insert User data.
    /// </summary>
    /// <param name="user">User.</param>
    Task<User> InsertUserAsync(User user);

    /// <summary>
    /// Base method for update data.
    /// </summary>
    /// <param name="user">User.</param>
    Task UpdateUserAsync(User user);

    /// <summary>
    /// Base method for delete data.
    /// </summary>
    /// <param name="id">id.</param>
    Task DeleteUserAsync(string id);

    /// <summary>
    /// Base method for populate data by email.
    /// </summary>
    /// <param name="email">email.</param>
    /// <returns><see cref="User"/>.</returns>
    Task<User> GetUserByEmailAsync(string email);

    /// <summary>
    /// Base method for populate all data with pagination, filters and sort.
    /// </summary>
    /// <returns>PaginatedList&lt;<see cref="User"/>&gt;.</returns>
    PagedList<ExpandoObject> GetUsers(QueryStringParameters queryStringParameters);

    /// <summary>
    /// Basic method for getting user by Id.
    /// </summary>
    /// <param name="id">id.</param>
    /// <returns><see cref="User"/>.</returns>
    Task<User> GetUserByIdAsync(string id);

    /// <summary>
    /// Basic method for getting user roles by Id.
    /// </summary>
    /// <param name="id">id.</param>
    /// <returns>IQueryable&lt;<see cref="string"/>&gt;.</returns>
    Task<IEnumerable<string>> GetRolesByIdAsync(string id);

    /// <summary>
    /// Basic method for check to equal passwords.
    /// </summary>
    /// <param name="id">id.</param>
    /// <param name="passwordHash">passwordHash.</param>
    /// <returns><see cref="bool"/>.</returns>
    Task<bool> CheckPasswordAsync(string id, string passwordHash);

    /// <summary>
    /// Base Method for populate data by Roles.
    /// </summary>
    /// <param name="roles">List of roles.</param>
    /// <returns>IQueryable&lt;<see cref="User"/>&gt;.</returns>
    IQueryable<User> GetAllUsersByRoles(string roles);
}
