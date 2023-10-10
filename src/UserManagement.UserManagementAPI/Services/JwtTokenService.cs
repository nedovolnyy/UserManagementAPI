namespace UserManagement.UserManagementAPI.Services;

public class JwtTokenService
{
    public string GenerateJwtToken(User user, IEnumerable<string> roles)
    {
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(JwtRegisteredClaimNames.Name, user.Name!),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        };
        userClaims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(userClaims),
            Issuer = Settings.Jwt.JwtIssuer,
            Audience = Settings.Jwt.JwtAudience,
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Jwt.JwtSecretKey)), SecurityAlgorithms.HmacSha512Signature),
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        ////var principal = new ClaimsPrincipal(new ClaimsIdentity(userClaims));

        ////Thread.CurrentPrincipal = principal;

        return jwtToken;
    }

    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            tokenHandler.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidIssuer = Settings.Jwt.JwtIssuer,
                ValidAudience = Settings.Jwt.JwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Jwt.JwtSecretKey)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                RoleClaimType = ClaimsIdentity.DefaultRoleClaimType,
            },
            out var _);
        }
        catch
        {
            return false;
        }

        return true;
    }
}
