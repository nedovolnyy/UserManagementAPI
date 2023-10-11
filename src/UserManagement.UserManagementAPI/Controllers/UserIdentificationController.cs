using UserManagement.UserManagementAPI.Helpers;

namespace UserManagement.Common.JwtTokenAuth.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserIdentificationController : ControllerBase
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<UserIdentificationController> _logger;
    private readonly IUserRepository _userRepository;

    public UserIdentificationController(
        JwtTokenService jwtTokenService,
        ILogger<UserIdentificationController> logger,
        IUserRepository userRepository)
    {
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Register new user.
    /// </summary>
    /// <param name="creatableUser">.</param>
    /// <returns>ActionResult.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET
    ///     {
    ///        "Name": "New Name",
    ///        "Age": 45,
    ///        "Email": "new@ema.il",
    ///        "Password": "****",
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request - Invalid payload.</response>
    /// <response code="500">Perhaps the validation failed.</response>
    [HttpPost("Register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterModel creatableUser)
    {
        _logger.LogInformation("Trying to register a new user.");

        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Invalid payload was provided.");

            return BadRequest(new AuthenticationResult
            {
                Result = false,
                Errors = new List<string>
                {
                    "Invalid payload",
                },
            });
        }

        await ValidationHelper.ValidateAsync(creatableUser, _userRepository);

        var existingUser = await _userRepository.GetUserByEmailAsync(creatableUser.Email);

        if (existingUser is not null)
        {
            _logger.LogInformation("The user with provided email already exists.");

            return BadRequest(new AuthenticationResult
            {
                Result = false,
                Errors = new List<string>
                {
                    "Email already exists",
                },
            });
        }

        var newUser = new User
        {
            Name = creatableUser.Name,
            Age = creatableUser.Age,
            Email = creatableUser.Email.Normalize(),
            PasswordHash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(creatableUser.Password))).Replace("-", string.Empty),
        };
        try
        {
            await _userRepository.InsertUserAsync(newUser);
        }
        catch
        {
            _logger.LogError("Something went wrong while creating a new user.");

            return new JsonResult(new AuthenticationResult
            {
                Result = false,
                Errors = new List<string> { "Something went wrong while creating a new user." },
            })
            { StatusCode = 500 };
        }

        var newUserEmail = newUser.Email;
        var roleName = (List<string>)await _userRepository.GetRolesByIdAsync(newUser.Id);
        var jwtToken = _jwtTokenService.GenerateJwtToken(newUser, roleName);

        _logger.LogInformation("The user with name {newUserEmail} was registered.", newUserEmail);

        return Ok(new AuthenticationResult
        {
            Result = true,
            Token = jwtToken,
            Roles = roleName.ConvertAll(
                delegate(string x)
                {
                    return (Roles)Enum.Parse(typeof(Roles), x);
                }),
        });
    }

    /// <summary>
    /// Login existing user.
    /// </summary>
    /// <param name="loginModel">.</param>
    /// <returns>ActionResult.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET
    ///     {
    ///        "Email": "user@example.com",
    ///        "Password": "string",
    ///     }.
    ///
    /// </remarks>
    /// <response code="200">Ok.</response>
    /// <response code="400">Bad request - Invalid payload.</response>
    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
    {
        var userEmail = loginModel.Email;
        _logger.LogInformation("Trying to login with user email {modelEmail}.", userEmail);

        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Invalid payload was provided.");

            return BadRequest(new AuthenticationResult
            {
                Result = false,
                Errors = new List<string>
                {
                    "Invalid payload",
                },
            });
        }

        var existingUser = await _userRepository.GetUserByEmailAsync(loginModel.Email);

        if (existingUser is null)
        {
            _logger.LogInformation("The user with provided email had not been found.");

            return BadRequest(new AuthenticationResult
            {
                Result = false,
                Errors = new List<string>
                {
                    "The user with this email does not exist.",
                },
            });
        }

        var existingUserName = existingUser.Name;
        var passwordHash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(loginModel.Password))).Replace("-", string.Empty);
        var isCorrect = await _userRepository.CheckPasswordAsync(existingUser.Id, passwordHash);

        if (!isCorrect)
        {
            _logger.LogInformation("The provided password is not correct for user {existingUserName}.", existingUserName);

            return BadRequest(new AuthenticationResult
            {
                Result = false,
                Errors = new List<string>
                {
                    "Password is not correct.",
                },
            });
        }

        var roleName = (List<string>)await _userRepository.GetRolesByIdAsync(existingUser.Id);
        var jwtToken = _jwtTokenService.GenerateJwtToken(existingUser, roleName);

        _logger.LogInformation("The user with email {userEmail} was logged in.", userEmail);
        HttpContext.Response.Cookies.Append("importantCookie", jwtToken);
        return Ok(new AuthenticationResult
        {
            Result = true,
            Token = jwtToken,
            User = existingUser,
            Roles = roleName.ConvertAll(
                delegate(string x)
                {
                    return (Roles)Enum.Parse(typeof(Roles), x);
                }),
        });
    }
}
