namespace UserManagement.UserManagementAPI.DataAccess;

public class ForMigrationContextFactory : IDesignTimeDbContextFactory<UserApiDbContext>
{
    public UserApiDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserApiDbContext>();

        var builder = new ConfigurationBuilder();
        builder.SetBasePath(Directory.GetCurrentDirectory());
        builder.AddJsonFile("appsettings.json");
        var config = builder.Build();

        var connectionString = config.GetConnectionString("DefaultConnection")!;
        optionsBuilder.UseSqlServer(connectionString, opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));
        return new UserApiDbContext(optionsBuilder.Options);
    }
}
