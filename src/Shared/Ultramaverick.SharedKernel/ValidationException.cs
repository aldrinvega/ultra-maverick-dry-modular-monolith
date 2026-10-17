namespace Ultramaverick.SharedKernel
{
    /// <summary>
    /// Thrown by a module's validation pipeline behavior when one or more validators fail.
    /// It lives in the shared kernel so the API's exception middleware can map failures from
    /// every module to the same 400 validation shape without referencing each module.
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
