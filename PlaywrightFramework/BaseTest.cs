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

    // O catálogo de objetos do app, pronto para o teste usar.
    //
    // Por que '=>' e não campo atribuído no InitializeAsync: a Page só nasce lá dentro, e
    // '=>' adia a construção para a hora do uso — o problema do "Page ainda não existe"
    // resolve sozinho. De quebra, cada propriedade devolve uma instância nova, e isso é
    // seguro justamente porque Page Object e Component NÃO guardam estado: um ILocator é a
    // descrição de como achar um elemento, não o elemento. Duas instâncias são idênticas.
    protected LoginPage LoginPage => new(Page);
    protected ProductsPage ProductsPage => new(Page);
    protected CartPage CartPage => new(Page);
    protected CheckoutInformationPage CheckoutInformationPage => new(Page);
    protected CheckoutOverviewPage CheckoutOverviewPage => new(Page);
    protected CheckoutCompletePage CheckoutCompletePage => new(Page);

    protected LoginFlow LoginFlow => new(Page);
    protected CheckoutFlow CheckoutFlow => new(Page);

    public async Task InitializeAsync()
    {
        _context = await browserFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = browserFixture.Settings.BaseUrl
        });

        Page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        // Fechar o context descarta todo o estado do teste. Nada vaza para o próximo.
        await _context.CloseAsync();
    }
}