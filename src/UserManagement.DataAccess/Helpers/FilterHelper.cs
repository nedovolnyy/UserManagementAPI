namespace UserManagement.DataAccess.Helpers;

internal class FilterHelper<T> : IFilterHelper<T>
{
    public IQueryable<T> ApplyFilter(IQueryable<T> entities, string filterByQueryString)
    {
        if (!entities.Any())
        {
            return entities;
        }

        if (string.IsNullOrWhiteSpace(filterByQueryString))
        {
            return entities;
        }

        return entities.Where(filterByQueryString);
    }
}
