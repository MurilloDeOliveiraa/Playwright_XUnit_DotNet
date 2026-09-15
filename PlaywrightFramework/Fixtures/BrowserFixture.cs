using Microsoft.Playwright;
using PlaywrightFramework.Configuration;

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

    public async Task InitializeAsync()
    {
        Settings = TestSettings.Load();

        PlaywrightDriver = await Playwright.CreateAsync();

        Browser = await PlaywrightDriver.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = Settings.Headless,
            SlowMo = Settings.SlowMoMilliseconds
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        PlaywrightDriver.Dispose();
    }
}
