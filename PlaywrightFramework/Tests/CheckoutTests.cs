using Microsoft.Playwright;
using PlaywrightFramework.Fixtures;
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

        var loginPage = new LoginPage(Page);
        var productsPage = new ProductsPage(Page);
        var cartPage = new CartPage(Page);
        var informationPage = new CheckoutInformationPage(Page);
        var overviewPage = new CheckoutOverviewPage(Page);
        var completePage = new CheckoutCompletePage(Page);

        await loginPage.GoToAsync();
        await loginPage.LoginAsAsync(order.User);
        foreach (var product in order.Products)
        {
            await productsPage.AddToCartAsync(product);
        }

        await productsPage.GoToCartAsync();
        await cartPage.StartCheckoutAsync();

        // Act
        await informationPage.FillInformationAsync(order.Delivery);
        await informationPage.ContinueAsync();
        await overviewPage.FinishAsync();

        // Assert
        await Assertions.Expect(completePage.ConfirmationHeader)
            .ToHaveTextAsync("Thank you for your order!");
    }

    [Fact]
    public async Task Checkout_SemCep_MostraErroDeValidacao()
    {
        // Arrange — o teste declara SÓ o desvio. Nome e sobrenome não são assunto dele.
        var order = new OrderBuilder().WithoutPostalCode().Build();

        var loginPage = new LoginPage(Page);
        var productsPage = new ProductsPage(Page);
        var cartPage = new CartPage(Page);
        var informationPage = new CheckoutInformationPage(Page);

        await loginPage.GoToAsync();
        await loginPage.LoginAsAsync(order.User);
        foreach (var product in order.Products)
        {
            await productsPage.AddToCartAsync(product);
        }

        await productsPage.GoToCartAsync();
        await cartPage.StartCheckoutAsync();

        // Act
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

        var loginPage = new LoginPage(Page);
        var productsPage = new ProductsPage(Page);
        var cartPage = new CartPage(Page);
        var informationPage = new CheckoutInformationPage(Page);
        var overviewPage = new CheckoutOverviewPage(Page);

        await loginPage.GoToAsync();
        await loginPage.LoginAsAsync(order.User);
        foreach (var product in order.Products)
        {
            await productsPage.AddToCartAsync(product);
        }

        await productsPage.GoToCartAsync();
        await cartPage.StartCheckoutAsync();

        // Act
        await informationPage.FillInformationAsync(order.Delivery);
        await informationPage.ContinueAsync();

        // Assert
        await Assertions.Expect(overviewPage.ItemNames).ToHaveCountAsync(2);
    }
}