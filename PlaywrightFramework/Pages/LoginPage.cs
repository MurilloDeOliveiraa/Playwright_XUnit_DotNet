using Microsoft.Playwright;
using PlaywrightFramework.TestData;

namespace PlaywrightFramework.Pages;

/// <summary>
/// POM da tela de login.
/// Seletores privados (mecânica) · ações públicas com nome de negócio (intenção).
/// O Page Object NÃO asserta — expõe estado (ErrorMessage) para o teste verificar.
/// </summary>
public class LoginPage
{
    private readonly IPage _page;

    private ILocator UsernameInput => _page.Locator("[data-test='username']");
    private ILocator PasswordInput => _page.Locator("[data-test='password']");
    private ILocator LoginButton => _page.Locator("[data-test='login-button']");

    /// <summary>Estado exposto para o teste assertar (web-first assertion).</summary>
    public ILocator ErrorMessage => _page.Locator("[data-test='error']");

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task GoToAsync()
    {
        await _page.GotoAsync("/");
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
