namespace MechanicShop.Client.Services
{
    public class ApiResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorDetail { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }

        public string? FirstErrorMessage =>
            ValidationErrors?.SelectMany(kvp => kvp.Value).FirstOrDefault() ?? ErrorMessage;

        public static ApiResult<T> Success(T data)
        {
            return new ApiResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        public static ApiResult<T> Failure(string? message, string? detail = null, int statusCode = 0, Dictionary<string, string[]>? validationErrors = null)
        {
            return new ApiResult<T>
            {
                IsSuccess = false,
                ErrorMessage = message,
                ErrorDetail = detail,
                StatusCode = statusCode,
                ValidationErrors = validationErrors
            };
        }
    }

    public class ApiResult : ApiResult<object>
    {
        public static ApiResult Success()
        {
            return new ApiResult
            {
                IsSuccess = true
            };
        }

        public new static ApiResult Failure(string? message, string? detail = null, int statusCode = 0, Dictionary<string, string[]>? validationErrors = null)
        {
            return new ApiResult
            {
                IsSuccess = false,
                ErrorMessage = message,
                ErrorDetail = detail,
                StatusCode = statusCode,
                ValidationErrors = validationErrors
            };
        }
    }
}