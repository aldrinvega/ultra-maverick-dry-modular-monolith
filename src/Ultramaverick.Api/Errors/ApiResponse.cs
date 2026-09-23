namespace Ultramaverick.Api.Errors
{
    /// <summary>Uniform error body: a status code and a human-readable message.</summary>
    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public ApiResponse(int statusCode, string? message = null)
        {
            StatusCode = statusCode;
            Message = message ?? DefaultMessage(statusCode);
        }

        private static string DefaultMessage(int statusCode) => statusCode switch
        {
            400 => "Bad request.",
            401 => "Unauthorized.",
            403 => "Forbidden.",
            404 => "Not found.",
            429 => "Too many requests.",
            _ => "An error occurred."
        };
    }
}
