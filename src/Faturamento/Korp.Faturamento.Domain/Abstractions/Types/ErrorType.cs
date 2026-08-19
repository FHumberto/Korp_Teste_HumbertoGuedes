namespace Korp.Faturamento.Domain.Abstractions.Types;

public enum ErrorType
{
    Failure = 500,
    Validation = 400,
    AccessUnauthorized = 401,
    AccessForbidden = 403,
    NotFound = 404,
    Conflict = 409
}
