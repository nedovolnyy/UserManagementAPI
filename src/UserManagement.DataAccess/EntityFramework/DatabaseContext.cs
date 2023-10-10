namespace UserManagement.DataAccess.EntityFramework;

public partial class DatabaseContext : DbContext, IDatabaseContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) => ConnectionString = Database.GetConnectionString();

    public string? ConnectionString { get; private set; }
    public DbContext Instance => this;
    public DbSet<User> Users { get; set; }
}
