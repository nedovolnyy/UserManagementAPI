var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(builder.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.File(builder.Configuration["Logging:FilePath"] !, rollingInterval: RollingInterval.Day)
                        .WriteTo.Console()
                        .CreateLogger();
builder.Host.UseSerilog(logger);

builder.WebHost.UseKestrel(options =>
{
    var configuration = (IConfiguration)options.ApplicationServices.GetService(typeof(IConfiguration))!;
    var httpsPort = configuration.GetValue("ASPNETCORE_HTTPS_PORT", 7001);
    var certPassword = configuration.GetValue<string>("CertPassword");
    var certPath = configuration.GetValue<string>("CertPath");

    options.Listen(IPAddress.Any, httpsPort, listenOptions =>
    {
        listenOptions.UseHttps(certPath!, certPassword);
    });
});

services.AddDbContext<UserApiDbContext>(
                options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), null))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

services.AddOpenApiDocument((options, provider) =>
{
    options.Title = "UserManagement API";
    options.Description = "ASP.NET Core 7.0 Web API for vebtech";
    options.DocumentName = $"UserManagement API v1.0";
    options.Version = "1.0.0";
    options.UseControllerSummaryAsTagDescription = true;

    options.AddSecurity("Bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = OpenApiSecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer ey.....\"",
    });
});

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = Settings.Jwt.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = Settings.Jwt.JwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.Jwt.JwtSecretKey)),
            ValidateLifetime = false,
            RoleClaimType = ClaimsIdentity.DefaultRoleClaimType,
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["importantCookie"];
                return Task.CompletedTask;
            },
        };
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
    })
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
    });

services.AddAuthorization(options =>
{
    foreach (var prop in Enum.GetValues<Roles>())
    {
        options.AddPolicy(nameof(prop), policy =>
        {
            policy.RequireClaim(ClaimsIdentity.DefaultRoleClaimType, Enum.GetNames<Roles>());
        });
    }
});

services.AddRepositories(builder.Configuration.GetConnectionString("DefaultConnection")!);
services.AddControllers()
    .AddNewtonsoftJson();
services.AddScoped<JwtTokenService>();

var app = builder.Build();

JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always,
});
app.UseSerilogRequestLogging();
app.UseOpenApi();
app.UseSwaggerUi3(options => options.DocExpansion = "list");
app.UseReDoc(options => options.Path = "/redoc");
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

app.Run();
