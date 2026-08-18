using Microsoft.Playwright;
using PlaywrightFramework.Components;

namespace PlaywrightFramework.Pages;

/// <summary>
/// Passo 3 do checkout: confirmação.
/// Classe magra de propósito — ela é a dona do locator da confirmação, e é isso que
/// impede o teste de saber que a mensagem mora num '.complete-header'.
/// </summary>
public class CheckoutCompletePage
{
    private readonly IPage _page;

    private ILocator BackHomeButton => _page.Locator("[data-test='back-to-products']");

    /// <summary>O cabeçalho compartilhado.</summary>
    public HeaderComponent Header { get; }

    /// <summary>Estado exposto: a mensagem de pedido concluído.</summary>
    public ILocator ConfirmationHeader => _page.Locator("[data-test='complete-header']");

    public CheckoutCompletePage(IPage page)
    {
        _page = page;
        Header = new HeaderComponent(page);
    }

    public async Task BackToProductsAsync()
    {
        await BackHomeButton.ClickAsync();
    }
}
