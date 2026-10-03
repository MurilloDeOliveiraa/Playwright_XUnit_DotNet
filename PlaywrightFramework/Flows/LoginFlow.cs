using Microsoft.Playwright;
using PlaywrightFramework.Pages;
using PlaywrightFramework.TestData;

namespace PlaywrightFramework.Flows;

/// <summary>
/// Jornada: entrar no sistema.
///
/// PÓS-CONDIÇÃO: depois desta chamada, o usuário ESTÁ dentro — e este fluxo CONFERE isso,
/// em vez de só prometer. Sem a conferência, ele devolvia o controle logo após clicar em Login,
/// e uma verificação negativa logo em seguida (ex.: "o carrinho não tem balão") passava por
/// motivo errado: a tela de produtos ainda nem existia.
///
/// Por isso este fluxo só serve para credenciais válidas. Com credenciais inválidas ele falha
/// rápido e diz o que esperava, em vez de devolver um sucesso que não aconteceu.
///
/// Sobre a regra "fluxo não faz Assert": o que ela protege é o VEREDITO sobre o comportamento
/// testado. Garantir a própria pós-condição não é veredito — se falhar, o problema é "não
/// consegui montar o cenário", e não "o sistema tem um defeito".
///
/// Quem NÃO usa este fluxo: LoginTests. Lá o login é o comportamento sob teste, e testar algo
/// através da abstração que existe para escondê-lo é como o teste emudece.
/// </summary>
public class LoginFlow(IPage page)
{
    private readonly LoginPage _loginPage = new(page);
    private readonly ProductsPage _productsPage = new(page);

    public async Task SignInAsAsync(User user)
    {
        await _loginPage.GoToAsync();
        await _loginPage.LoginAsAsync(user);

        // A pós-condição. O título identifica QUAL tela é — o cabeçalho sozinho existe em
        // todas as telas logadas e não provaria isso.
        await Assertions.Expect(_productsPage.Header.Title).ToHaveTextAsync("Products");
    }
}
