using Microsoft.Playwright;
using PlaywrightFramework.Fixtures;
using PlaywrightFramework.Flows;
using PlaywrightFramework.Pages;
using Xunit.Abstractions;

namespace PlaywrightFramework;

/// <summary>
/// Classe-base dos testes de UI.
/// Regra-mãe: UM browser por execução (vem do fixture), UM context por teste (criado aqui).
/// Context é barato e é o que dá isolamento real: cookies, localStorage e sessão zerados
/// a cada teste — sem nenhuma limpeza manual.
/// </summary>
[Collection(PlaywrightCollection.Name)] //Mesma coisa que escrever a string "Playwright Collection"
public abstract class BaseTest(BrowserFixture browserFixture, ITestOutputHelper testOutputHelper) //Primary Constructor, já declara e inicializa as variáveis
    : IAsyncLifetime
{
    private IBrowserContext _context = null!;

    protected IPage Page { get; private set; } = null!;

    protected ITestOutputHelper TestOutputHelper { get; } = testOutputHelper;

    protected LoginPage LoginPage = null!;
    protected ProductsPage ProductsPage = null!;
    protected CartPage CartPage = null!;
    protected CheckoutCompletePage CheckoutCompletePage = null!;
    protected CheckoutInformationPage CheckoutInformationPage = null!;
    protected CheckoutOverviewPage CheckoutOverviewPage = null!;

    protected LoginFlow LoginFlow = null!;
    protected CheckoutFlow CheckoutFlow = null!;

    public async Task InitializeAsync()
    {
        _context = await browserFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = "https://www.saucedemo.com"
        });

        Page = await _context.NewPageAsync();
        LoginPage = new LoginPage(Page);
        ProductsPage = new ProductsPage(Page);
        CartPage = new CartPage(Page);
        CheckoutCompletePage = new CheckoutCompletePage(Page);
        CheckoutInformationPage = new CheckoutInformationPage(Page);
        CheckoutOverviewPage = new CheckoutOverviewPage(Page);
        
        LoginFlow =  new LoginFlow(Page);
        CheckoutFlow = new CheckoutFlow(Page);
    }

    public async Task DisposeAsync()
    {
        // Fechar o context descarta todo o estado do teste. Nada vaza para o próximo.
        await _context.CloseAsync();
    }
}