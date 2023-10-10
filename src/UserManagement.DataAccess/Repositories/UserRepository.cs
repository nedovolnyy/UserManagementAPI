namespace UserManagement.DataAccess.Repositories;

internal class UserRepository : IUserRepository
{
    private readonly IDatabaseContext _databaseContext;
    private readonly DbSet<User> _dbSet;
    private readonly IFilterHelper<User> _filterHelper;
    private readonly ISortHelper<User> _sortHelper;
    private readonly IDataShaper<User> _dataShaper;

    public UserRepository(IDatabaseContext databaseContext, ISortHelper<User> sortHelper,
                          IDataShaper<User> dataShaper, IFilterHelper<User> filterHelper)
    {
        _databaseContext = databaseContext;
        _dbSet = _databaseContext.Instance.Set<User>();
        _filterHelper = filterHelper;
        _sortHelper = sortHelper;
        _dataShaper = dataShaper;
    }

    public virtual async Task<User> InsertUserAsync(User user)
    {
        await _dbSet.AddAsync(user);
        await _databaseContext.Instance.SaveChangesAsync();
        return await GetUserByEmailAsync(user.Email);
    }

    public virtual async Task UpdateUserAsync(User user)
    {
        _databaseContext.Instance.Entry(user).State = EntityState.Modified;
        await _databaseContext.Instance.SaveChangesAsync();
    }

    public virtual async Task DeleteUserAsync(string id)
    {
        _dbSet.Remove((await _dbSet.FindAsync(id))!);
        await _databaseContext.Instance.SaveChangesAsync();
    }

    public virtual async Task<User> GetUserByEmailAsync(string email)
        => (await _dbSet.Where(p => p.Email == email).FirstOrDefaultAsync())!;

    public virtual PagedList<ExpandoObject> GetUsers(QueryStringParameters queryStringParameters)
    {
        var filterUsers = _filterHelper.ApplyFilter(_dbSet, queryStringParameters.FilterExpression!);
        var sortedUsers = _sortHelper.ApplySort(filterUsers, queryStringParameters.OrderBy);
        var shapedOwners = _dataShaper.ShapeData(sortedUsers, queryStringParameters.DisplayedFields!);
        return PagedList<ExpandoObject>.Create(shapedOwners, queryStringParameters.PageNumber, queryStringParameters.PageSize);
    }

    public virtual async Task<User> GetUserByIdAsync(string id)
        => (await _dbSet.FindAsync(id))!;

    public virtual async Task<IEnumerable<string>> GetRolesByIdAsync(string id)
        => (await _dbSet.FindAsync(id))!.Roles!.Split(',').ToList();

    public virtual async Task<bool> CheckPasswordAsync(string id, string passwordHash)
        => (await _dbSet.FindAsync(id))!.PasswordHash == passwordHash;

    public virtual IQueryable<User> GetAllUsersByRoles(string roles)
        => _databaseContext.Users.Where(p => p.Roles == roles);
}
