using Microsoft.Playwright;
using PlaywrightFramework.Fixtures;
using PlaywrightFramework.Flows;
using PlaywrightFramework.Pages;
using PlaywrightFramework.TestData;
using Xunit.Abstractions;

namespace PlaywrightFramework.Tests;

/// <summary>
/// Estes dois testes existem para DEMONSTRAR isolamento.
/// Se o context fosse compartilhado, o carrinho do primeiro teste vazaria para o segundo
/// e o "carrinho vazio" quebraria (badge com 1 em vez de nenhum badge).
/// Como o BaseTest cria um context novo por teste, os dois passam em qualquer ordem.
/// </summary>
public class CartTests(BrowserFixture browserFixture, ITestOutputHelper testOutputHelper)
    : BaseTest(browserFixture, testOutputHelper)
{
    [Fact]
    public async Task AdicionarUmProduto_MostraBadgeComUm()
    {
        // Arrange — aqui o login é PRÉ-CONDIÇÃO, não o alvo: usa o fluxo.
        await LoginFlow.SignInAsAsync(UsersFactory.Standard());
        
        // Act
        await ProductsPage.ItemNamed("Sauce Labs Backpack").AddToCartAsync();

        // Assert
        await Assertions.Expect(ProductsPage.Header.CartBadge).ToHaveTextAsync("1");
    }

    [Fact]
    public async Task NovoTeste_ComecaComCarrinhoVazio()
    {
        // Arrange & Act
        await LoginFlow.SignInAsAsync(UsersFactory.Standard());

        // Assert — sem badge nenhum: nada vazou do teste anterior.
        await Assertions.Expect(ProductsPage.Header.CartBadge).Not.ToBeVisibleAsync();
    }
}
