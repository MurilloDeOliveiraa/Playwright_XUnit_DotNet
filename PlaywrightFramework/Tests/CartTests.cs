using Microsoft.Playwright;
using PlaywrightFramework.Fixtures;
using PlaywrightFramework.Pages;
using PlaywrightFramework.TestData;
using Xunit.Abstractions;

namespace PlaywrightFramework.Tests;

/// <summary>
/// Estes dois testes existem para DEMONSTRAR isolamento.
/// Se o context fosse compartilhado, o carrinho do primeiro teste vazaria para o segundo
/// e o "carrinho vazio" quebraria (badge com 1 em vez de nenhum badge).
/// Como o PageTest cria um context novo por teste, os dois passam em qualquer ordem.
/// </summary>
public class CartTests(BrowserFixture browserFixture, ITestOutputHelper testOutputHelper)
    : BaseTest(browserFixture, testOutputHelper)
{

    [Fact]
    public async Task AdicionarUmProduto_MostraBadgeComUm()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        var productsPage = new ProductsPage(Page);
        await loginPage.GoToAsync();
        await loginPage.LoginAsAsync(UsersFactory.Standard());

        // Act
        await productsPage.AddToCartAsync("Sauce Labs Backpack");

        // Assert
        await Assertions.Expect(productsPage.CartBadge).ToHaveTextAsync("1");
    }

    [Fact]
    public async Task NovoTeste_ComecaComCarrinhoVazio()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        var productsPage = new ProductsPage(Page);
        await loginPage.GoToAsync();

        // Act
        await loginPage.LoginAsAsync(UsersFactory.Standard());

        // Assert — sem badge nenhum: nada vazou do teste anterior.
        await Assertions.Expect(productsPage.CartBadge).Not.ToBeVisibleAsync();
    }
}
