using Microsoft.Playwright;

namespace PlaywrightFramework.Components;

/// <summary>
/// O cabeçalho que aparece em TODA tela logada: menu, carrinho e título da tela.
///
/// A diferença para um Page Object está no construtor: um componente é ancorado num
/// ELEMENTO (o 'Root'), não na página inteira. Toda busca acontece dentro dessa raiz.
/// Para o cabeçalho isso parece exagero — só existe um — mas é a mesma mecânica que faz
/// o InventoryItemComponent funcionar quando o componente se repete na tela.
///
/// Note quem NÃO tem este componente: LoginPage. A tela de login não tem cabeçalho nenhum,
/// então 'loginPage.Header' simplesmente não existe — e não compila.
/// </summary>
public class HeaderComponent(IPage page)
{
    private ILocator Root => page.Locator("#header_container"); //Locator raíz - os outros locators começarão a partir dele
    private ILocator MenuButton => Root.Locator("#react-burger-menu-btn");
    private ILocator LogoutLink => Root.Locator("[data-test='logout-sidebar-link']");
    private ILocator ResetAppStateLink => Root.Locator("[data-test='reset-sidebar-link']");
    private ILocator CartLink => Root.Locator("[data-test='shopping-cart-link']");

    /// <summary>Estado exposto: o balãozinho com a contagem do carrinho.</summary>
    public ILocator CartBadge => Root.Locator("[data-test='shopping-cart-badge']");

    /// <summary>Estado exposto: o título da tela atual ("Products", "Checkout: Overview"...).</summary>
    public ILocator Title => Root.Locator("[data-test='title']");

    public async Task GoToCartAsync()
    {
        await CartLink.ClickAsync();
    }

    public async Task LogoutAsync()
    {
        // O menu precisa ser aberto: os links existem no DOM, mas ficam fora da tela.
        await MenuButton.ClickAsync();
        await LogoutLink.ClickAsync();
    }

    public async Task ResetAppStateAsync()
    {
        await MenuButton.ClickAsync();
        await ResetAppStateLink.ClickAsync();
    }
}
