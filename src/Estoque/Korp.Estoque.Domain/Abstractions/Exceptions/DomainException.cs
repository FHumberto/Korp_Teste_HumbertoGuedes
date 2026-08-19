using Korp.Estoque.Domain.Abstractions.Types;

namespace Korp.Estoque.Domain.Abstractions.Exceptions;

public sealed class DomainException : Exception
{
    #region [ PROPRIEDADES ]

    public Error Error { get; }

    #endregion

    #region [ CONSTRUTORES ]

    public DomainException(Error error) : base(GetDescription(error)) => Error = error;

    #endregion

    #region [ MÉTODOS PRIVADOS ]

    private static string GetDescription(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return error.Description;
    }

    #endregion
}
