using Microsoft.Playwright;
using PlaywrightFramework.Fixtures;
using Xunit.Abstractions;

namespace PlaywrightFramework;

/// <summary>
/// Classe-base dos testes de UI.
/// Regra-mãe: UM browser por execução (vem do fixture), UM context por teste (criado aqui).
/// Context é barato e é o que dá isolamento real: cookies, localStorage e sessão zerados
/// a cada teste — sem nenhuma limpeza manual.
/// </summary>
[Collection(PlaywrightCollection.Name)]
public abstract class BaseTest : IAsyncLifetime
{
    private readonly BrowserFixture _browserFixture;

    private IBrowserContext _context = null!;

    protected IPage Page { get; private set; } = null!;

    protected ITestOutputHelper _testOutputHelper;

    protected BaseTest(BrowserFixture browserFixture, ITestOutputHelper testOutputHelper)
    {
        _browserFixture = browserFixture;
        _testOutputHelper = testOutputHelper;
    }

    public async Task InitializeAsync()
    {
        _context = await _browserFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = "https://www.saucedemo.com"
        });

        Page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        // Fechar o context descarta todo o estado do teste. Nada vaza para o próximo.
        await _context.CloseAsync();
    }
}
