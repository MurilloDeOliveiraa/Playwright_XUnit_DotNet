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
        await LoginPage.GoToAsync();

        // Act
        await LoginPage.LoginAsAsync(UsersFactory.Standard());

        // Assert — web-first: espera sozinha, sem sleep.
        await Assertions.Expect(ProductsPage.Header.Title).ToHaveTextAsync("Products");
    }

    [Fact]
    public async Task Login_ComSenhaErrada_MostraMensagemDeErro()
    {
        // Arrange
        await LoginPage.GoToAsync();

        // Act
        await LoginPage.LoginAsAsync(UsersFactory.WithWrongPassword());

        // Assert
        await Assertions.Expect(LoginPage.ErrorMessage)
            .ToContainTextAsync("Username and password do not match");
    }

    [Fact]
    public async Task Login_ComUsuarioBloqueado_MostraMensagemDeBloqueio()
    {
        // Arrange
        await LoginPage.GoToAsync();

        // Act — o teste diz "usuário bloqueado", não "a string locked_out_user".
        await LoginPage.LoginAsAsync(UsersFactory.LockedOut());

        // Assert
        await Assertions.Expect(LoginPage.ErrorMessage)
            .ToContainTextAsync("this user has been locked out");
    }
}