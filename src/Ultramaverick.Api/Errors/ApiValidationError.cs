namespace Ultramaverick.Api.Errors
{
    /// <summary>Validation failure body: the base error shape plus the individual errors.</summary>
    public class ApiValidationError : ApiResponse
    {
        public IEnumerable<string> Errors { get; set; }

        public ApiValidationError() : base(400)
        {
            Errors = Array.Empty<string>();
        }

        public ApiValidationError(IEnumerable<string> errors) : base(400)
        {
            Errors = errors;
        }
    }
}
