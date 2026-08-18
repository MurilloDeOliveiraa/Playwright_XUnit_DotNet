using Microsoft.Playwright;
using PlaywrightFramework.Fixtures;
using PlaywrightFramework.Flows;
using PlaywrightFramework.Pages;
using PlaywrightFramework.TestData;
using Xunit.Abstractions;

namespace PlaywrightFramework.Tests;

public class CheckoutTests(BrowserFixture browserFixture, ITestOutputHelper testOutputHelper)
    : BaseTest(browserFixture, testOutputHelper)
{
    [Fact]
    public async Task Checkout_ComDadosValidos_ConcluiPedido()
    {
        // Arrange
        var order = new OrderBuilder().Build(); // um pedido normal, sem nenhum desvio

        // Act — aqui a jornada inteira É o comportamento sob teste.
        await CheckoutFlow.PlaceOrderAsync(order);

        // Assert
        await Assertions.Expect(CheckoutCompletePage.ConfirmationHeader)
            .ToHaveTextAsync("Thank you for your order!");
    }

    [Fact]
    public async Task Checkout_SemCep_MostraErroDeValidacao()
    {
        // Arrange — o fluxo cobre a pré-condição e para na porta.
        var order = new OrderBuilder().WithoutPostalCode().Build();
        await CheckoutFlow.GoToDeliveryInformationAsync(order);

        // Act — o que está sendo testado, e só isso.
        await CheckoutInformationPage.FillInformationAsync(order.Delivery);
        await CheckoutInformationPage.ContinueAsync();

        // Assert
        await Assertions.Expect(CheckoutInformationPage.ErrorMessage)
            .ToContainTextAsync("Postal Code is required");
    }

    [Fact]
    public async Task Checkout_NaTelaDeEntrega_BadgeAindaMostraOsItens()
    {
        // Arrange — este teste era IMPOSSÍVEL antes do HeaderComponent: o badge morava
        // dentro do ProductsPage, e aqui a gente está na tela de dados de entrega.
        var order = new OrderBuilder()
            .WithProduct("Sauce Labs Backpack")
            .WithProduct("Sauce Labs Bike Light")
            .Build();

        // Act
        await CheckoutFlow.GoToDeliveryInformationAsync(order);

        // Assert
        await Assertions.Expect(CheckoutInformationPage.Header.CartBadge).ToHaveTextAsync("2");
    }

    [Fact]
    public async Task Checkout_ComDoisProdutos_ListaAmbosNoResumo()
    {
        // Arrange — aqui o desvio é a quantidade de produtos.
        var order = new OrderBuilder()
            .WithProduct("Sauce Labs Backpack")
            .WithProduct("Sauce Labs Bike Light")
            .Build();

        // Act
        await CheckoutFlow.GoToOrderSummaryAsync(order);

        // Assert
        await Assertions.Expect(CheckoutOverviewPage.ItemNames).ToHaveCountAsync(2);
    }
}
