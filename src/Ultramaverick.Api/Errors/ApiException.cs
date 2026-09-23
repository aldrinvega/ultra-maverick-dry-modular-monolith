namespace Ultramaverick.Api.Errors
{
    /// <summary>Unhandled-error body: the base error shape plus details (development only).</summary>
    public class ApiException : ApiResponse
    {
        public string? Details { get; set; }

        public ApiException(int statusCode, string? message = null, string? details = null)
            : base(statusCode, message)
        {
            Details = details;
        }
    }
}
