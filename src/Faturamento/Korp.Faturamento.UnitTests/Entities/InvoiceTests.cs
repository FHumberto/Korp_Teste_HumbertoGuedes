using Korp.Faturamento.Domain.Abstractions.Exceptions;
using Korp.Faturamento.Domain.Abstractions.Types;
using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Entities.Errors;
using Korp.Faturamento.Domain.Enums;
using Shouldly;

namespace Korp.Faturamento.UnitTests.Entities;

public sealed class InvoiceTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 8, 19, 12, 0, 0, TimeSpan.Zero);

    // Cobre "Cadastro de Notas Fiscais": numeração definida e status inicial Aberta.
    [Fact]
    public void Create_ShouldStartOpen()
    {
        Guid id = Guid.NewGuid();

        Invoice? invoice = Invoice.Create(id, 1, CreatedAt);

        invoice.Id.ShouldBe(id);
        invoice.Number.ShouldBe(1);
        invoice.Status.ShouldBe(InvoiceStatus.Open);
        invoice.Items.ShouldBeEmpty();
        invoice.CreatedAt.ShouldBe(CreatedAt);
        invoice.ClosedAt.ShouldBeNull();
    }

    // Cobre "Cadastro de Notas Fiscais": cada produto incluído deve possuir uma quantidade utilizável.
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddItem_WithNonPositiveQuantity_ShouldFail(int quantity)
    {
        Invoice invoice = CreateInvoice();

        void action() => AddItem(invoice, Guid.NewGuid(), quantity);

        ShouldThrowDomainError(action, InvoiceItemErrors.InvalidQuantity);
        invoice.Items.ShouldBeEmpty();
    }

    // Apoia "Inclusão de múltiplos produtos": mantém cada produto como um único item coerente na nota.
    [Fact]
    public void AddItem_WithDuplicateProduct_ShouldFail()
    {
        Invoice invoice = CreateInvoice();
        var productId = Guid.NewGuid();
        AddItem(invoice, productId);

        void action() => AddItem(invoice, productId);

        ShouldThrowDomainError(action, InvoiceErrors.DuplicateProduct);
        invoice.Items.Count.ShouldBe(1);
    }

    // Cobre "Impressão de Notas Fiscais": uma nota Fechada não pode voltar a ser alterada.
    [Fact]
    public void AddItem_WhenInvoiceIsClosed_ShouldFail()
    {
        Invoice invoice = CreateInvoice();
        AddItem(invoice, Guid.NewGuid());
        invoice.Close(CreatedAt.AddMinutes(1));

        void action() => AddItem(invoice, Guid.NewGuid());

        ShouldThrowDomainError(action, InvoiceErrors.ClosedModification);
        invoice.Items.Count.ShouldBe(1);
    }

    // Cobre "Cadastro de Notas Fiscais": uma nota precisa conter produtos com suas quantidades.
    [Fact]
    public void Close_WithoutItems_ShouldFail()
    {
        Invoice invoice = CreateInvoice();

        void action() => invoice.Close(CreatedAt.AddMinutes(1));

        ShouldThrowDomainError(action, InvoiceErrors.WithoutItems);
        invoice.Status.ShouldBe(InvoiceStatus.Open);
        invoice.ClosedAt.ShouldBeNull();
    }

    // Cobre "Impressão de Notas Fiscais": após a finalização, o status é atualizado para Fechada.
    [Fact]
    public void Close_WhenInvoiceIsOpen_ShouldClose()
    {
        Invoice invoice = CreateInvoice();
        AddItem(invoice, Guid.NewGuid());
        DateTimeOffset closedAt = CreatedAt.AddMinutes(1);

        invoice.Close(closedAt);

        invoice.Status.ShouldBe(InvoiceStatus.Closed);
        invoice.ClosedAt.ShouldBe(closedAt);
    }

    // Cobre "Impressão de Notas Fiscais": não permite novo processamento quando o status não é Aberta.
    [Fact]
    public void Close_WhenInvoiceIsClosed_ShouldFail()
    {
        Invoice invoice = CreateInvoice();
        AddItem(invoice, Guid.NewGuid());
        DateTimeOffset firstClosedAt = CreatedAt.AddMinutes(1);
        invoice.Close(firstClosedAt);

        void action() => invoice.Close(CreatedAt.AddMinutes(2));

        ShouldThrowDomainError(action, InvoiceErrors.AlreadyClosed);
        invoice.Status.ShouldBe(InvoiceStatus.Closed);
        invoice.ClosedAt.ShouldBe(firstClosedAt);
    }

    // Cobre "Cadastro de Notas Fiscais": preserva o produto e a quantidade incluídos na nota.
    [Fact]
    public void AddItem_WithValidData_ShouldPreserveProductSnapshot()
    {
        Invoice invoice = CreateInvoice();
        Guid productId = Guid.NewGuid();

        invoice.AddItem(productId, "PROD-001", "Produto de demonstração", 2);

        InvoiceItem item = invoice.Items.ShouldHaveSingleItem();
        item.Id.ShouldNotBe(Guid.Empty);
        item.ProductId.ShouldBe(productId);
        item.ProductCode.ShouldBe("PROD-001");
        item.ProductDescription.ShouldBe("Produto de demonstração");
        item.Quantity.ShouldBe(2);
    }

    // Apoia "Serviço de Faturamento – gestão de notas fiscais": impede alteração externa dos itens.
    [Fact]
    public void Items_ShouldNotAllowExternalMutation()
    {
        Invoice invoice = CreateInvoice();
        AddItem(invoice, Guid.NewGuid());

        ICollection<InvoiceItem> items = invoice.Items.ShouldBeAssignableTo<ICollection<InvoiceItem>>();

        items.IsReadOnly.ShouldBeTrue();
    }

    // Apoia "Inclusão de múltiplos produtos": cada item deve identificar um produto cadastrado.
    [Fact]
    public void AddItem_WithEmptyProductId_ShouldFail()
    {
        Invoice invoice = CreateInvoice();

        void action() => AddItem(invoice, Guid.Empty);

        ShouldThrowDomainError(action, InvoiceItemErrors.ProductIdRequired);
        invoice.Items.ShouldBeEmpty();
    }

    // Apoia a integração Estoque–Faturamento: o snapshot do item exige o código do produto.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddItem_WithBlankProductCode_ShouldFail(string productCode)
    {
        Invoice invoice = CreateInvoice();

        void action() => invoice.AddItem(
            Guid.NewGuid(),
            productCode,
            "Produto de demonstração",
            1);

        ShouldThrowDomainError(action, InvoiceItemErrors.ProductCodeRequired);
        invoice.Items.ShouldBeEmpty();
    }

    // Apoia a integração Estoque–Faturamento: o snapshot do item exige a descrição do produto.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddItem_WithBlankProductDescription_ShouldFail(string productDescription)
    {
        Invoice invoice = CreateInvoice();

        void action() => invoice.AddItem(
            Guid.NewGuid(),
            "PROD-001",
            productDescription,
            1);

        ShouldThrowDomainError(action, InvoiceItemErrors.ProductDescriptionRequired);
        invoice.Items.ShouldBeEmpty();
    }

    // Cobre "Cadastro de Notas Fiscais": a numeração sequencial deve representar uma nota válida.
    [Fact]
    public void Create_WithNonPositiveNumber_ShouldFail()
    {
        static void action() => Invoice.Create(Guid.NewGuid(), 0, CreatedAt);

        ShouldThrowDomainError(action, InvoiceErrors.InvalidNumber);
    }

    // Apoia "Conexão Real com banco de dados": exige identidade válida para persistir e consultar a nota.
    [Fact]
    public void Create_WithEmptyId_ShouldFail()
    {
        static void action() => Invoice.Create(Guid.Empty, 1, CreatedAt);

        ShouldThrowDomainError(action, InvoiceErrors.IdRequired);
    }

    // Apoia a integridade do produto incluído na nota, exigida no cadastro de notas fiscais.
    [Fact]
    public void AddItem_WithProductCodeLongerThanLimit_ShouldFail()
    {
        Invoice invoice = CreateInvoice();
        string productCode = new('A', InvoiceItem.MaxProductCodeLength + 1);

        void action() => invoice.AddItem(Guid.NewGuid(), productCode, "Produto de demonstração", 1);

        ShouldThrowDomainError(action, InvoiceItemErrors.ProductCodeTooLong);
        invoice.Items.ShouldBeEmpty();
    }

    // Apoia a integridade do produto incluído na nota, exigida no cadastro de notas fiscais.
    [Fact]
    public void AddItem_WithProductDescriptionLongerThanLimit_ShouldFail()
    {
        Invoice invoice = CreateInvoice();
        string productDescription = new(
            'A',
            InvoiceItem.MaxProductDescriptionLength + 1);

        void action() => invoice.AddItem(
            Guid.NewGuid(),
            "PROD-001",
            productDescription,
            1);

        ShouldThrowDomainError(action, InvoiceItemErrors.ProductDescriptionTooLong);
        invoice.Items.ShouldBeEmpty();
    }

    private static Invoice CreateInvoice() => Invoice.Create(Guid.NewGuid(), 1, CreatedAt);

    private static void AddItem(Invoice invoice, Guid productId, int quantity = 1) => invoice.AddItem(productId, "PROD-001", "Produto de demonstração", quantity);

    private static void ShouldThrowDomainError(Action action, Error expectedError)
    {
        DomainException exception = action.ShouldThrow<DomainException>();

        exception.Error.ShouldBe(expectedError);
        exception.Message.ShouldBe(expectedError.Description);
    }
}
