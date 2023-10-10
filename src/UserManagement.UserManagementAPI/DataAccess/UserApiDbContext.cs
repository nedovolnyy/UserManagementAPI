namespace UserManagement.UserManagementAPI.DataAccess;

public partial class UserApiDbContext : DbContext, IDatabaseContext
{
    public UserApiDbContext(DbContextOptions<UserApiDbContext> options)
        : base(options) => ConnectionString = Database.GetConnectionString();

    public string? ConnectionString { get; private set; }
    public DbContext Instance => this;
    public DbSet<User> Users { get; set; }
}
