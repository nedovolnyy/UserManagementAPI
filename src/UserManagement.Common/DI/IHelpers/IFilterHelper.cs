namespace UserManagement.Common.DI.IHelpers;

public interface IFilterHelper<T>
{
    IQueryable<T> ApplyFilter(IQueryable<T> entities, string filterByQueryString);
}
