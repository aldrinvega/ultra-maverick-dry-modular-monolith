using FluentValidation;
using MediatR;
using AppValidationException = Ultramaverick.Identity.Application.Exceptions.ValidationException;

namespace Ultramaverick.Identity.Application.Behaviors
{
    /// <summary>
    /// Runs every registered FluentValidation validator for a request before its handler.
    /// Without this, validators are registered but never executed.
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(validator => validator.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .Select(failure => failure.ErrorMessage)
                .ToArray();

            if (failures.Length > 0)
                throw new AppValidationException(failures);

            return await next();
        }
    }
}
