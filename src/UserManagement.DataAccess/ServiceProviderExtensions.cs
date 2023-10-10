namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceProviderExtensions
{
    public static void AddRepositories(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<IDatabaseContext, DatabaseContext>(
            options => options.UseSqlServer(connectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IFilterHelper<User>, FilterHelper<User>>();
        services.AddTransient<ISortHelper<User>, SortHelper<User>>();
        services.AddTransient<IDataShaper<User>, DataShaper<User>>();
    }
}