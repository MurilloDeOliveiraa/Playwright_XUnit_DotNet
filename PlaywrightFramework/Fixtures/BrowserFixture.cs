using Microsoft.Playwright;

namespace PlaywrightFramework.Fixtures;

/// <summary>
/// Sobe o browser UMA vez para a suite inteira.
/// Browser é caro (processo do SO): abrir/fechar por teste custaria segundos por teste.
/// IAsyncLifetime existe porque construtor não pode ser async — e todo Playwright é async.
/// </summary>
public class BrowserFixture : IAsyncLifetime
{
    public IPlaywright PlaywrightDriver { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PlaywrightDriver = await Playwright.CreateAsync();

        Browser = await PlaywrightDriver.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.CloseAsync();
        PlaywrightDriver.Dispose();
    }
}
