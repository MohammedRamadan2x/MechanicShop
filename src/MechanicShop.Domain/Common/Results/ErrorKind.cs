namespace MechanicShop.Domain.Common.Results;

public enum ErrorKind
{
    Failure,
    Unexpected,
    Validation,
    Conflict,
    NotFound,
    Unauthorized,
    Forbidden,
}
