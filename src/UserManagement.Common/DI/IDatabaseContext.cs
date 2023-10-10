namespace UserManagement.Common.DI;

public interface IDatabaseContext
{
    string? ConnectionString { get; }
    DbContext Instance { get; }
    DbSet<User> Users { get; set; }
}