namespace Ultramaverick.Identity.Application.Exceptions
{
    /// <summary>
    /// Thrown by the validation pipeline behavior when one or more validators fail.
    /// The API maps it to a 400 with the shared validation error shape.
    /// </summary>
    public sealed class ValidationException : Exception
    {
        public IReadOnlyList<string> Errors { get; }

        public ValidationException(IReadOnlyList<string> errors)
            : base("One or more validation errors occurred.")
        {
            Errors = errors;
        }
    }
}
