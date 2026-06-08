using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Validation.Abstractions;
using CustomCodeFramework.Validation.Mappers;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CustomCodeFramework.Validation.FluentValidation;

public sealed class FluentValidationService(IServiceProvider serviceProvider) : IValidationService
{
    public async Task<Result> ValidateAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var validators = serviceProvider.GetServices<IValidator<TRequest>>().ToArray();

        if (validators.Length == 0)
        {
            return Result.Success();
        }

        var validationTasks = validators.Select(validator =>
            validator.ValidateAsync(request, cancellationToken)
        );

        var validationResults = await Task.WhenAll(validationTasks);

        var errors = validationResults.SelectMany(result => result.ToValidationErrors()).ToArray();

        if (errors.Length == 0)
        {
            return Result.Success();
        }

        return Result.Failure(ValidationResultMapper.ToError(errors));
    }
}
