using Microsoft.Playwright;
using PlaywrightFramework.Components;
using PlaywrightFramework.TestData;

namespace PlaywrightFramework.Pages;

/// <summary>
/// Passo 1 do checkout: dados de entrega.
/// Repare que Continue NÃO devolve a próxima página — a validação pode falhar e você
/// continua aqui mesmo. Devolver CheckoutOverviewPage seria uma mentira de tipo.
/// </summary>
public class CheckoutInformationPage
{
    private readonly IPage _page;

    private ILocator FirstNameInput => _page.Locator("[data-test='firstName']");
    private ILocator LastNameInput => _page.Locator("[data-test='lastName']");
    private ILocator PostalCodeInput => _page.Locator("[data-test='postalCode']");
    private ILocator ContinueButton => _page.Locator("[data-test='continue']");

    /// <summary>O cabeçalho compartilhado.</summary>
    public HeaderComponent Header { get; }

    /// <summary>Estado exposto: erro de validação dos dados de entrega.</summary>
    public ILocator ErrorMessage => _page.Locator("[data-test='error']");

    public CheckoutInformationPage(IPage page)
    {
        _page = page;
        Header = new HeaderComponent(page);
    }

    /// <summary>
    /// Recebe o DeliveryInfo inteiro: acaba com o risco de trocar a ordem de três
    /// strings posicionais (sobrenome no lugar do CEP compilava numa boa).
    /// </summary>
    public async Task FillInformationAsync(DeliveryInfo delivery)
    {
        await FirstNameInput.FillAsync(delivery.FirstName);
        await LastNameInput.FillAsync(delivery.LastName);
        await PostalCodeInput.FillAsync(delivery.PostalCode);
    }

    public async Task ContinueAsync()
    {
        await ContinueButton.ClickAsync();
    }
}
