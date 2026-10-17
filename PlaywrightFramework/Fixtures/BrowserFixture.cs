using Microsoft.Playwright;
using PlaywrightFramework.Configuration;
using PlaywrightFramework.Evidence;

namespace PlaywrightFramework.Fixtures;

/// <summary>
/// Sobe o browser UMA vez para a suite inteira.
/// Browser é caro (processo do SO): abrir/fechar por teste custaria segundos por teste.
/// IAsyncLifetime existe porque construtor não pode ser async — e todo Playwright é async.
///
/// A configuração é lida aqui pelo mesmo motivo: ler arquivo custa, e o valor não muda
/// entre um teste e outro. Uma leitura por execução, igual ao browser.
/// </summary>
public class BrowserFixture : IAsyncLifetime
{
    public IPlaywright PlaywrightDriver { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public TestSettings Settings { get; private set; } = null!;

    /// <summary>
    /// Identifica ESTA execução (data e hora, até o segundo). Tirado uma vez só, junto com o browser:
    /// todas as falhas da mesma rodada caem na mesma pasta de evidência.
    /// </summary>
    public string RunId { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Settings = TestSettings.Load();
        RunId = DateTime.Now.ToString(FailureEvidence.RunIdFormat);
        FailureEvidence.DeleteOldRuns(Settings);
        Assertions.SetDefaultExpectTimeout(Settings.ExpectTimeout);

        PlaywrightDriver = await Playwright.CreateAsync();

        Browser = await BrowserTypeFor(Settings.Browser).LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Settings.Headless,
            SlowMo = Settings.SlowMoMilliseconds
        });
    }

    // Sem ramo padrão de propósito: se alguém acrescentar um navegador a BrowserKind e esquecer
    // de ensinar o caso aqui, a suíte falha alto em vez de rodar em outro navegador sem avisar.
    private IBrowserType BrowserTypeFor(BrowserKind kind) => kind switch
    {
        BrowserKind.Chromium => PlaywrightDriver.Chromium,
        BrowserKind.Firefox => PlaywrightDriver.Firefox,
        BrowserKind.Webkit => PlaywrightDriver.Webkit,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Navegador sem suporte no BrowserFixture.")
    };

    public async ValueTask DisposeAsync()
    {
        // Se a subida falhou (ex.: nome de navegador inválido), nada foi criado — e fechar o que
        // não existe geraria um segundo erro que esconde o verdadeiro.
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        PlaywrightDriver?.Dispose();
    }
}
