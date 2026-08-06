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
        var checkoutFlow = new CheckoutFlow(Page);
        var completePage = new CheckoutCompletePage(Page);

        // Act — aqui a jornada inteira É o comportamento sob teste.
        await checkoutFlow.PlaceOrderAsync(order);

        // Assert
        await Assertions.Expect(completePage.ConfirmationHeader)
            .ToHaveTextAsync("Thank you for your order!");
    }

    [Fact]
    public async Task Checkout_SemCep_MostraErroDeValidacao()
    {
        // Arrange — o fluxo cobre a pré-condição e para na porta.
        var order = new OrderBuilder().WithoutPostalCode().Build();
        var checkoutFlow = new CheckoutFlow(Page);
        var informationPage = new CheckoutInformationPage(Page);

        await checkoutFlow.GoToDeliveryInformationAsync(order);

        // Act — o que está sendo testado, e só isso.
        await informationPage.FillInformationAsync(order.Delivery);
        await informationPage.ContinueAsync();

        // Assert
        await Assertions.Expect(informationPage.ErrorMessage)
            .ToContainTextAsync("Postal Code is required");
    }

    [Fact]
    public async Task Checkout_ComDoisProdutos_ListaAmbosNoResumo()
    {
        // Arrange — aqui o desvio é a quantidade de produtos.
        var order = new OrderBuilder()
            .WithProduct("Sauce Labs Backpack")
            .WithProduct("Sauce Labs Bike Light")
            .Build();

        var checkoutFlow = new CheckoutFlow(Page);
        var overviewPage = new CheckoutOverviewPage(Page);

        // Act
        await checkoutFlow.GoToOrderSummaryAsync(order);

        // Assert
        await Assertions.Expect(overviewPage.ItemNames).ToHaveCountAsync(2);
    }
}
