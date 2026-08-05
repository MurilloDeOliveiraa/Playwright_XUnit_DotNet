using Microsoft.Playwright;

namespace PlaywrightFramework.Pages;

/// <summary>
/// POM da tela de produtos (o inventário do SauceDemo).
/// Expõe CartBadge como estado para o teste assertar quantos itens estão no carrinho.
/// </summary>
public class ProductsPage
{
    private readonly IPage _page;

    private ILocator Title => _page.Locator(".title");
    private ILocator CartLink => _page.Locator("[data-test='shopping-cart-link']");

    /// <summary>Estado exposto: o "balãozinho" com a contagem do carrinho.</summary>
    public ILocator CartBadge => _page.Locator("[data-test='shopping-cart-badge']");

    /// <summary>Estado exposto: o título da tela ("Products") — prova que o login passou.</summary>
    public ILocator Heading => Title;

    public ProductsPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Nome de negócio, não mecânica: o teste diz "adiciona a mochila",
    /// não "clica no botão add-to-cart-sauce-labs-backpack".
    /// </summary>
    public async Task AddToCartAsync(string productName)
    {
        await AddToCartButtonFor(productName).ClickAsync();
    }

    public async Task GoToCartAsync()
    {
        await CartLink.ClickAsync();
    }

    // O SauceDemo monta o data-test a partir do nome do produto em kebab-case.
    // Essa tradução é mecânica → fica privada aqui dentro.
    private ILocator AddToCartButtonFor(string productName)
    {
        var slug = productName.Trim().ToLowerInvariant().Replace(' ', '-');
        return _page.Locator($"[data-test='add-to-cart-{slug}']");
    }
}
