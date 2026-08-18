using Microsoft.Playwright;
using PlaywrightFramework.TestData;

namespace PlaywrightFramework.Pages;

/// <summary>
/// POM da tela de login.
/// Seletores privados (mecânica) · ações públicas com nome de negócio (intenção).
/// O Page Object NÃO asserta — expõe estado (ErrorMessage) para o teste verificar.
/// </summary>
public class LoginPage(IPage page) //Primary Constructor, já declara e inicializa as variáveis
{
    private ILocator UsernameInput => page.Locator("[data-test='username']");
    private ILocator PasswordInput => page.Locator("[data-test='password']");
    private ILocator LoginButton => page.Locator("[data-test='login-button']");

    /// <summary>Estado exposto para o teste assertar (web-first assertion).</summary>
    public ILocator ErrorMessage => page.Locator("[data-test='error']");

    public async Task GoToAsync()
    {
        await page.GotoAsync("/");
    }

    /// <summary>
    /// Recebe um User inteiro, não duas strings soltas: elimina a chance de inverter
    /// os argumentos e deixa o call-site legível (LoginAsAsync(Users.LockedOut())).
    /// </summary>
    public async Task LoginAsAsync(User user)
    {
        await UsernameInput.FillAsync(user.Username);
        await PasswordInput.FillAsync(user.Password);
        await LoginButton.ClickAsync();
    }
}
