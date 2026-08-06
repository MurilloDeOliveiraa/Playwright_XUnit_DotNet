using Microsoft.Playwright;
using PlaywrightFramework.Pages;
using PlaywrightFramework.TestData;

namespace PlaywrightFramework.Flows;

/// <summary>
/// Jornada: entrar no sistema.
///
/// PÓS-CONDIÇÃO: depois desta chamada, o usuário ESTÁ dentro. Por isso este fluxo só recebe
/// credenciais válidas — chamá-lo esperando falha quebraria a promessa do nome.
///
/// Repare quem NÃO usa este fluxo: LoginTests. Lá o login é o comportamento sob teste, e
/// testar algo através da abstração que existe para escondê-lo é como o teste emudece:
/// no dia em que este fluxo virar login por storageState/API, o LoginTests continuaria
/// verde sem nunca mais tocar na tela de login.
/// </summary>
public class LoginFlow(IPage page)
{
    private readonly LoginPage _loginPage = new(page);

    public async Task SignInAsAsync(User user)
    {
        await _loginPage.GoToAsync();
        await _loginPage.LoginAsAsync(user);
    }
}
