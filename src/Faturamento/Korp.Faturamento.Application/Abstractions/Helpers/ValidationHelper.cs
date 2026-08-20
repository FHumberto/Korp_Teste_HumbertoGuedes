using FluentValidation.Results;

namespace Korp.Faturamento.Application.Abstractions.Helpers;

public static class ValidationHelper
{
    public static Error ToValidationError(ValidationResult validationResult)
    {
        Dictionary<string, string[]> errors = validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return Error.Validation("VALIDATION_ERROR", "Ocorreram erros de validação.", errors);
    }
}
