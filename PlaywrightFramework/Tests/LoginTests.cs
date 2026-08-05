using Microsoft.Playwright;
using PlaywrightFramework.Fixtures;
using PlaywrightFramework.Pages;
using PlaywrightFramework.TestData;
using Xunit.Abstractions;

namespace PlaywrightFramework.Tests;

public class LoginTests(BrowserFixture browserFixture, ITestOutputHelper testOutputHelper)
    : BaseTest(browserFixture, testOutputHelper)
{
    [Fact]
    public async Task Login_ComCredenciaisValidas_LevaParaProdutos()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        var productsPage = new ProductsPage(Page);
        await loginPage.GoToAsync();

        var user1 = new User("Murillo", "Lopes");
        _testOutputHelper.WriteLine(user1.ToString()); //Como o User é um record, ele possui um método ToString() que printa as propriedades de uma forma melhor.

        // Act
        await loginPage.LoginAsAsync(UsersFactory.Standard());

        // Assert — web-first: espera sozinha, sem sleep.
        await Assertions.Expect(productsPage.Heading).ToHaveTextAsync("Products");
    }

    [Fact]
    public async Task Login_ComSenhaErrada_MostraMensagemDeErro()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        await loginPage.GoToAsync();

        // Act
        await loginPage.LoginAsAsync(UsersFactory.WithWrongPassword());

        // Assert
        await Assertions.Expect(loginPage.ErrorMessage)
            .ToContainTextAsync("Username and password do not match");
    }

    [Fact]
    public async Task Login_ComUsuarioBloqueado_MostraMensagemDeBloqueio()
    {
        // Arrange
        var loginPage = new LoginPage(Page);
        await loginPage.GoToAsync();

        // Act — o teste diz "usuário bloqueado", não "a string locked_out_user".
        await loginPage.LoginAsAsync(UsersFactory.LockedOut());

        // Assert
        await Assertions.Expect(loginPage.ErrorMessage)
            .ToContainTextAsync("this user has been locked out");
    }
}