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
        _logger.LogInformation("Returned {UserCount} users from database.", userCount);
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
    /// <response code="400">Bad request - cretableUser is null.</response>
    [HttpPost("user")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> CreateUserAsync(User cretableUser, string password)
    {
        if (cretableUser is null)
        {
            return BadRequest();
        }

        var newUser = new User
        {
            Name = cretableUser.Name,
            Age = cretableUser.Age,
            Email = cretableUser.Email.Normalize(),
            PasswordHash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(password))).Replace("-", string.Empty),
            Roles = cretableUser.Roles,
        };
        return Ok(await _userRepository.InsertUserAsync(newUser));
    }

    /// <summary>
    /// Updates the specified <paramref name="updatableUser"/> in the backing store.
    /// </summary>
    /// <param name="updatableUser">The user to update.</param>
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
    ///        "Password": "newPassword"
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request - cretableUser is null.</response>
    [HttpPut("user")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<User>> UpdateUserAsync(User updatableUser)
    {
        var existingUser = (await _userRepository.GetUserByIdAsync(updatableUser.Id!))!;
        if (existingUser is null)
        {
            return BadRequest();
        }

        var updatedUser = new User
        {
            Name = updatableUser.Name,
            Age = updatableUser.Age,
            Email = updatableUser.Email,
            PasswordHash = updatableUser.PasswordHash,
            Roles = updatableUser.Roles,
        };

        await _userRepository.UpdateUserAsync(updatedUser);
        return Ok(updatedUser);
    }

    /// <summary>
    /// Delete selected user.
    /// </summary>
    /// <returns>.</returns>
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
            await _userRepository.DeleteUserAsync(id);
            return Ok();
        }

        return NotFound();
    }

    /// <summary>
    /// Returns selected user.
    /// </summary>
    /// <returns>.</returns>
    [HttpGet("user/{userId}")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> GetByIdUserAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Returns selected users role.
    /// </summary>
    /// <returns>.</returns>
    [HttpGet("role/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetRolesByIdAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user.Roles);
    }

    /// <summary>
    /// Change role selected user.
    /// </summary>
    /// <returns>.</returns>
    [HttpPut("role/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeRolesAsync(string userId, IEnumerable<string> roles)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user is null)
        {
            return BadRequest();
        }

        user.Roles = string.Join(",", roles.ToArray());
        await _userRepository.UpdateUserAsync(user);

        return Ok();
    }
}