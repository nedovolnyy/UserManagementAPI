namespace UserManagement.Common.Validation;

[Serializable]
public class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }

    protected ValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
