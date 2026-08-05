using Microsoft.Playwright;

namespace PlaywrightFramework.Pages;

/// <summary>POM do carrinho: lista o que foi adicionado e dá a partida no checkout.</summary>
public class CartPage
{
    private readonly IPage _page;

    private ILocator CheckoutButton => _page.Locator("[data-test='checkout']");

    /// <summary>Estado exposto: os nomes dos itens no carrinho.</summary>
    public ILocator ItemNames => _page.Locator("[data-test='inventory-item-name']");

    public CartPage(IPage page)
    {
        _page = page;
    }

    public async Task StartCheckoutAsync()
    {
        await CheckoutButton.ClickAsync();
    }
}
