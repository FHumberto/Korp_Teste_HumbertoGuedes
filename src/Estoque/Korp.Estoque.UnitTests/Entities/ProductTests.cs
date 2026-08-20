using Korp.Estoque.Domain.Abstractions.Types;
using Korp.Estoque.Domain.Entities.Errors;
using Korp.Estoque.Domain.Exceptions;

namespace Korp.Estoque.UnitTests.Entities;

public sealed class ProductTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    // Cobre "Cadastro de Produtos": o saldo representa a quantidade disponível e não pode nascer inválido.
    [Fact]
    public void Create_WithNegativeBalance_ShouldFail()
    {
        static void action() => CreateProduct(initialBalance: -1);

        ShouldThrowDomainError(action, ProductErrors.NegativeBalance);
    }

    // Cobre "Cadastro de Produtos": cadastra código, descrição e saldo para uso posterior nas notas.
    [Fact]
    public void Create_WithValidData_ShouldSetStateAndLeaveUpdatedAtNull()
    {
        var id = Guid.NewGuid();

        Product product = CreateProduct(id: id);

        product.Id.ShouldBe(id);
        product.Code.ShouldBe("PROD-001");
        product.Description.ShouldBe("Produto de demonstração");
        product.Balance.ShouldBe(10);
        product.CreatedAt.ShouldBe(CreatedAt);
        product.UpdatedAt.ShouldBeNull();
    }

    // Cobre "Impressão de Notas Fiscais": a atualização do saldo exige uma quantidade efetivamente utilizada.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Debit_WithNonPositiveQuantity_ShouldFail(int quantity)
    {
        Product product = CreateProduct();

        void action() => product.Debit(quantity, CreatedAt.AddMinutes(1));

        ShouldThrowDomainError(action, ProductErrors.InvalidDebitQuantity);
        product.Balance.ShouldBe(10);
        product.UpdatedAt.ShouldBeNull();
    }

    // Cobre "Impressão de Notas Fiscais": a baixa não pode produzir um saldo disponível negativo.
    [Fact]
    public void Debit_WithInsufficientBalance_ShouldFail()
    {
        Product product = CreateProduct(initialBalance: 1);

        void action() => product.Debit(2, CreatedAt.AddMinutes(1));

        ShouldThrowDomainError(action, ProductErrors.InsufficientStock);
        product.Balance.ShouldBe(1);
        product.UpdatedAt.ShouldBeNull();
    }

    // Cobre o exemplo do desafio: saldo anterior 10 menos a quantidade utilizada 2 resulta em saldo 8.
    [Fact]
    public void Debit_WithAvailableBalance_ShouldReduceBalance()
    {
        Product product = CreateProduct(initialBalance: 10);
        DateTimeOffset updatedAt = CreatedAt.AddMinutes(1);

        product.Debit(2, updatedAt);

        product.Balance.ShouldBe(8);
        product.UpdatedAt.ShouldBe(updatedAt);
    }

    // Cobre "Cadastro de Produtos": Código é um campo obrigatório.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankCode_ShouldFail(string code)
    {
        void action() => CreateProduct(code: code);

        ShouldThrowDomainError(action, ProductErrors.CodeRequired);
    }

    // Cobre "Cadastro de Produtos": Descrição é um campo obrigatório.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankDescription_ShouldFail(string description)
    {
        void action() => CreateProduct(description: description);

        ShouldThrowDomainError(action, ProductErrors.DescriptionRequired);
    }

    // Apoia "Cadastro de Produtos": protege a integridade do código persistido para uso nas notas.
    [Fact]
    public void Create_WithCodeLongerThanLimit_ShouldFail()
    {
        static void action() => CreateProduct(code: new string('A', Product.MaxCodeLength + 1));

        ShouldThrowDomainError(action, ProductErrors.CodeTooLong);
    }

    // Apoia "Cadastro de Produtos": protege a integridade da descrição persistida.
    [Fact]
    public void Create_WithDescriptionLongerThanLimit_ShouldFail()
    {
        static void action() => CreateProduct(description: new string('A', Product.MaxDescriptionLength + 1));

        ShouldThrowDomainError(action, ProductErrors.DescriptionTooLong);
    }

    // Apoia "Conexão Real com banco de dados": exige identidade válida para persistir e consultar o produto.
    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        static void action() => CreateProduct(id: Guid.Empty);

        ShouldThrowDomainError(action, ProductErrors.IdRequired);
    }

    private static Product CreateProduct
    (
        Guid? id = null,
        string code = "PROD-001",
        string description = "Produto de demonstração",
        int initialBalance = 10) => Product.Create
        (
            id ?? Guid.NewGuid(),
            code,
            description,
            initialBalance,
            CreatedAt
    );

    private static void ShouldThrowDomainError(Action action, Error expectedError)
    {
        DomainException exception = action.ShouldThrow<DomainException>();

        exception.Error.ShouldBe(expectedError);
        exception.Message.ShouldBe(expectedError.Description);
    }
}
