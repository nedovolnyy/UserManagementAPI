namespace UserManagement.DataAccess.Helpers;

internal class SortHelper<T> : ISortHelper<T>
{
    public IQueryable<T> ApplySort(IQueryable<T> entities, string orderByQueryString)
    {
        if (!entities.Any())
        {
            return entities;
        }

        if (string.IsNullOrWhiteSpace(orderByQueryString))
        {
            return entities;
        }

        var entityParams = orderByQueryString.Trim().Split(',');
        var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var entityQueryBuilder = new StringBuilder();

        foreach (var param in entityParams)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                continue;
            }

            var propertyFromQueryName = param.Split(" ")[0];
            var objectProperty = Array.Find(propertyInfos, p => p.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));

            if (objectProperty is null)
            {
                continue;
            }

            var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";

            entityQueryBuilder.Append($"{objectProperty.Name.ToString()} {sortingOrder}, ");
        }

        var orderQuery = entityQueryBuilder.ToString().TrimEnd(',', ' ');

        return entities.OrderBy(orderQuery);
    }
}
