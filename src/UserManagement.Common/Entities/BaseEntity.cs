namespace UserManagement.Common.Entities;

public class BaseEntity
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
}
