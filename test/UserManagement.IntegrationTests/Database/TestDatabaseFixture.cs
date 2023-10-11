namespace UserManagement.IntegrationTests.Database;

[SetUpFixture]
internal class TestDatabaseFixture : WebApplicationFactory<Program>
{
    private IServiceScope _scope;

    internal static IServiceProvider ServiceProvider { get; private set; }
    internal static IDatabaseContext DatabaseContext { get; private set; }
    public static HttpClient Client { get; private set; } = null!;
    private WebApplicationFactory<Program> WebApplicationFactory { get; set; } = null!;
    private IConfiguration Configuration { get; set; } = null!;

    [OneTimeSetUp]
    public async Task Setup()
    {
        WebApplicationFactory = new WebApplicationFactory<Program>()
    .WithWebHostBuilder(builder =>
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("testconfig.json", true)
            .Build();
        builder.UseConfiguration(config);
        builder.ConfigureServices(services =>
        {
            services.AddRepositories(config.GetConnectionString("DefaultConnection"));
        });
    });
        Client = WebApplicationFactory.CreateClient();
        Configuration = WebApplicationFactory.Services.GetRequiredService<IConfiguration>();
        _scope = WebApplicationFactory.Services.CreateScope();
        ServiceProvider = _scope.ServiceProvider;
        DatabaseContext = ServiceProvider.GetRequiredService<IDatabaseContext>();
        AssertionOptions.FormattingOptions.MaxLines = 500;
        await InitiallizeDatabase();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await DropDatabase();
        Dispose(true);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            WebApplicationFactory?.Dispose();
            _scope?.Dispose();
            Client?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        return base.CreateHost(builder);
    }

    public async Task InitiallizeDatabase()
    {
        await DropDatabase();

        var target = new DacpacService();
        target.ProcessDacPac(DatabaseContext.ConnectionString,
                             Configuration["Database:DefaultDatabaseName"],
                             Configuration["Database:DefaultDatabaseFileName"]);
    }

    public async Task DropDatabase() => await DatabaseContext.Instance.Database.EnsureDeletedAsync();
}
