using Microsoft.Playwright;

namespace PlaywrightFramework.Pages;

/// <summary>Passo 2 do checkout: resumo do pedido antes de confirmar.</summary>
public class CheckoutOverviewPage
{
    private readonly IPage _page;

    private ILocator FinishButton => _page.Locator("[data-test='finish']");

    /// <summary>Estado exposto: os itens que entraram no pedido.</summary>
    public ILocator ItemNames => _page.Locator("[data-test='inventory-item-name']");

    /// <summary>Estado exposto: o total com impostos.</summary>
    public ILocator Total => _page.Locator("[data-test='total-label']");

    public CheckoutOverviewPage(IPage page)
    {
        _page = page;
    }

    public async Task FinishAsync()
    {
        await FinishButton.ClickAsync();
    }
}
