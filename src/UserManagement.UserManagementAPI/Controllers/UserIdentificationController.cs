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
    /// <returns>..</returns>
    [HttpPost("Register")]
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
    /// <param name="model">.</param>
    /// <returns>..</returns>
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var userEmail = model.Email;
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

        var existingUser = await _userRepository.GetUserByEmailAsync(model.Email);

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
        var passwordHash = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(model.Password))).Replace("-", string.Empty);
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
