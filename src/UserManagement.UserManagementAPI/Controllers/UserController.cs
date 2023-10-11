using UserManagement.UserManagementAPI.Helpers;

namespace UserManagement.UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = nameof(Roles.Admin) + "," + nameof(Roles.SuperAdmin))]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserRepository userRepository, ILogger<UserController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Returns list of the users with params of pagination.
    /// </summary>
    /// <param name="queryStringParams">queryStringParams.</param>
    /// <returns>A newly created PagedList.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET
    ///     {
    ///        "PageNumber": 1,
    ///        "PageSize": 2,
    ///        "FilterExpression": "Age>18 &amp; Roles="SuperAdmin"",
    ///        "OrderBy": "Name",
    ///        "DisplayedFields": "Id, Name, Age, Roles",
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">If the item is null.</response>
    [HttpGet("users")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedList<ExpandoObject>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public PagedList<ExpandoObject> GetPagedListOfUsers([FromQuery] QueryStringParameters queryStringParams)
    {
        var users = _userRepository.GetUsers(queryStringParams);
        var userCount = users.Items.Count;
        _logger!.LogInformation("Returned {UserCount} users from database.", userCount);
        return users;
    }

    /// <summary>
    /// Add new user.
    /// </summary>
    /// <returns>.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST
    ///     {
    ///        "Id": -,
    ///        "Name": "New Name",
    ///        "Age": 45,
    ///        "Email": "new@ema.il",
    ///        "PasswordHash": -,
    ///        "Roles": -,
    ///        "Password": "newPassword"
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request - user is null.</response>
    /// <response code="500">Perhaps the validation failed.</response>
    [HttpPost("user")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> CreateUserAsync(User cretableUser, string password)
    {
        if (cretableUser is null)
        {
            _logger!.LogInformation("Cannot create user! СretableUser is null");
            return BadRequest();
        }

        await ValidationHelper.ValidateAsync(cretableUser, _userRepository);
        _logger!.LogInformation("Validation was successful.");

        var newUser = new User
        {
            Name = cretableUser.Name,
            Age = cretableUser.Age,
            Email = cretableUser.Email.Normalize(),
            PasswordHash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).Replace("-", string.Empty),
            Roles = cretableUser.Roles,
        };
        _logger!.LogInformation("Everything is ready to create a user.");

        return Ok(await _userRepository.InsertUserAsync(newUser));
    }

    /// <summary>
    /// Updates the specified <paramref name="updatableUser"/> in the backing store.
    /// </summary>
    /// <param name="updatableUser">The user to update.</param>
    /// <param name="newPassword">New password.</param>
    /// <returns>
    /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="ActionResult"/>
    /// of the operation.
    /// </returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT
    ///     {
    ///        "Id": "??",
    ///        "Name": "New Name",
    ///        "Age": 45,
    ///        "Email": "new@ema.il",
    ///        "PasswordHash": "??",
    ///        "Roles": "??",
    ///        "newPassword": "newPassword"
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request - cretableUser is null.</response>
    /// <response code="500">Perhaps the validation failed.</response>
    [HttpPut("user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<User>> UpdateUserAsync(User updatableUser, string newPassword = "")
    {
        var existingUser = (await _userRepository.GetUserByIdAsync(updatableUser.Id!))!;
        if (existingUser is null)
        {
            _logger!.LogInformation("Cannot update user! UpdatableUser is null");
            return BadRequest();
        }

        await ValidationHelper.ValidateAsync(updatableUser, _userRepository);
        _logger!.LogInformation("Validation was successful.");

        var hasPasswordChanged = updatableUser.PasswordHash != existingUser.PasswordHash;
        var password = hasPasswordChanged && !string.IsNullOrEmpty(newPassword)
                ? BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(newPassword))).Replace("-", string.Empty)
                : existingUser.PasswordHash;

        var updatedUser = new User
        {
            Name = updatableUser.Name,
            Age = updatableUser.Age,
            Email = updatableUser.Email,
            PasswordHash = password,
            Roles = updatableUser.Roles,
        };

        _logger!.LogInformation("Everything is ready to update a user.");
        await _userRepository.UpdateUserAsync(updatedUser);

        _logger!.LogInformation("Users data has been updated.");
        return Ok(updatedUser);
    }

    /// <summary>
    /// Delete selected user.
    /// </summary>
    /// <returns>ActionResult.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE
    ///     {
    ///        "Id": "??",
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="401">If you aren't authorize.</response>
    /// <response code="404">If the item is null.</response>
    [HttpDelete("user/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteUserAsync(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user is not null)
        {
            _logger!.LogInformation("Everything is ready to delete a user.");
            await _userRepository.DeleteUserAsync(id);

            _logger!.LogInformation("User has been deleted.");
            return Ok();
        }

        _logger!.LogInformation("Cannot delete user! He don't exist with this: {id}!", id);
        return NotFound();
    }

    /// <summary>
    /// Returns selected user.
    /// </summary>
    /// <param name="userId">userId.</param>
    /// <returns>Existed User, if any.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET
    ///     {
    ///        "userId": "sfsdfs-dfsdf-sdsfsdfsdf",
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">If the item is null.</response>
    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<User>> GetByIdUserAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        _logger!.LogInformation("Users data has been read.");
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Returns selected users role.
    /// </summary>
    /// <param name="userId">userId.</param>
    /// <returns>Roles as a string, of existed User, if any.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET
    ///     {
    ///        "userId": "sfsdfs-dfsdf-sdsfsdfsdf",
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="404">If the item is null.</response>
    [HttpGet("role/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetRolesByIdAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger!.LogInformation("User don't exist with id: {userId}", userId);
            return NotFound();
        }

        _logger!.LogInformation("Users data has been read.");
        return Ok(user.Roles);
    }

    /// <summary>
    /// Change role selected user.
    /// </summary>
    /// <param name="userId">userId.</param>
    /// <param name="roles">roles.</param>
    /// <returns>ActionResult.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET
    ///     {
    ///        "userId": "sfsdfs-dfsdf-sdsfsdfsdf",
    ///        "roles": "SuperAdmin,User,Admin",
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">If the item is null.</response>
    [HttpPut("role/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeRolesAsync(string userId, IEnumerable<string> roles)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger!.LogInformation("User don't exist with id: {userId}", userId);
            return BadRequest();
        }

        _logger!.LogInformation("Users data has been read.");
        user.Roles = string.Join(",", roles.ToArray());
        await _userRepository.UpdateUserAsync(user);

        _logger!.LogInformation("Roles of user has been changed.");
        return Ok();
    }
}