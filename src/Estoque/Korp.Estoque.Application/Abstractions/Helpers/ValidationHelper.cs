using FluentValidation.Results;
using Korp.Estoque.Domain.Abstractions.Types;

namespace Korp.Estoque.Application.Abstractions.Helpers;

public static class ValidationHelper
{
    /// <summary>
    /// Converte um <see cref="ValidationResult"/> em um objeto <see cref="Error"/> contendo os erros agrupados por propriedade.
    /// </summary>
    /// <param name="validationResult">Resultado da validação contendo os erros.</param>
    /// <returns>Objeto <see cref="Error.Validation(string, string, IDictionary{string, string[]}?)"/>.</returns>
    public static Error ToValidationError(ValidationResult validationResult)
    {
        Dictionary<string, string[]> errorDictionary = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(e => e.ErrorMessage).ToArray()
            );

        return Error.Validation
        (
            code: "VALIDATION_ERROR",
            description: "Ocorreram erros de validação.",
            validationDetails: errorDictionary
        );
    }
}
