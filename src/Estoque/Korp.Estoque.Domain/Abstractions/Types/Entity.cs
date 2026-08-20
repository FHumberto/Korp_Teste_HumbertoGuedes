namespace Korp.Estoque.Domain.Abstractions.Types;

public abstract class Entity
{
    #region [ PROPRIEDADES ]

    public Guid Id { get; private set; }

    #endregion

    #region [ CONSTRUTORES ]

    protected Entity() { }

    protected Entity(Guid id) => Id = id;

    #endregion
}
