namespace Korp.Faturamento.Domain.Abstractions.Types;

public sealed class Error
{
    #region [ PROPRIEDADES ]

    public string Code { get; }
    public string Description { get; }
    public ErrorType ErrorType { get; }
    public IReadOnlyDictionary<string, string[]>? ValidationDetails { get; }

    #endregion

    #region [ CONSTRUTORES ]

    private Error(string code, string description, ErrorType errorType, IDictionary<string, string[]>? validationDetails = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Code = code;
        Description = description;
        ErrorType = errorType;
        ValidationDetails = validationDetails?.ToDictionary(detail => detail.Key, detail => detail.Value.ToArray());
    }

    #endregion

    #region [ CRIAÇÃO ]

    public static Error Failure(string code, string description)
        => new(code, description, ErrorType.Failure);

    public static Error Validation(string code, string description, IDictionary<string, string[]>? validationDetails = null)
        => new(code, description, ErrorType.Validation, validationDetails);

    public static Error AccessUnauthorized(string code, string description)
        => new(code, description, ErrorType.AccessUnauthorized);

    public static Error AccessForbidden(string code, string description)
        => new(code, description, ErrorType.AccessForbidden);

    public static Error NotFound(string code, string description)
        => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description)
        => new(code, description, ErrorType.Conflict);

    public static Error ServiceUnavailable(string code, string description)
        => new(code, description, ErrorType.ServiceUnavailable);

    #endregion
}
