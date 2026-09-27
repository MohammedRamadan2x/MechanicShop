namespace MechanicShop.Domain.Common.Results;

public readonly record struct Error
{
    public string Code { get; }

    public string Description { get; }

    public ErrorKind Type { get; }

    private Error(string code, string description, ErrorKind type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static Error Failure(
        string code = nameof(Failure), 
        string description = "General failure.")
        => new(code, description, ErrorKind.Failure);

    public static Error Unexpected(
        string code = nameof(Unexpected), 
        string description = "Unexpected error.")
        => new(code, description, ErrorKind.Unexpected);

    public static Error Validation(
        string code = nameof(Validation), 
        string description = "Validation error")
        => new(code, description, ErrorKind.Validation);

    public static Error Conflict(
        string code = nameof(Conflict), 
        string description = "Conflict error")
        => new(code, description, ErrorKind.Conflict);

    public static Error NotFound(
        string code = nameof(NotFound), 
        string description = "Not found error")
        => new(code, description, ErrorKind.NotFound);

    public static Error Unauthorized(
        string code = nameof(Unauthorized), 
        string description = "Unauthorized error")
        => new(code, description, ErrorKind.Unauthorized);

    public static Error Forbidden(
        string code = nameof(Forbidden), 
        string description = "Forbidden error")
        => new(code, description, ErrorKind.Forbidden);

    public static Error Create(int type, string code, string description)
        => new(code, description, (ErrorKind)type);
}
