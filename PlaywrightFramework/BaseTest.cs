using Microsoft.Playwright;
using PlaywrightFramework.Configuration;
using PlaywrightFramework.Evidence;
using PlaywrightFramework.Fixtures;
using PlaywrightFramework.Flows;
using PlaywrightFramework.Pages;

namespace PlaywrightFramework;

/// <summary>
/// Classe-base dos testes de UI.
/// Regra-mãe: UM browser por execução (vem do fixture), UM context por teste (criado aqui).
/// Context é barato e é o que dá isolamento real: cookies, localStorage e sessão zerados
/// a cada teste — sem nenhuma limpeza manual.
///
/// Sem [Collection]: cada classe de teste é a própria coleção, e o xUnit roda coleções em paralelo.
/// O browser único chega pelo BrowserFixture, que é um fixture de ASSEMBLY (ver BrowserFixture).
/// </summary>
public abstract class BaseTest(BrowserFixture browserFixture, ITestOutputHelper testOutputHelper) //Primary Constructor, já declara e inicializa as variáveis
    : IAsyncLifetime
{
    private IBrowserContext _context = null!;
    private FailureEvidence _evidence = null!;

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

    public async ValueTask InitializeAsync()
    {
        // O relatório do xUnit não sabe o navegador (o nome do teste é só classe+método), então
        // o teste o escreve na própria saída, passando ou falhando. Só o navegador: o caminho da
        // evidência continua sendo escrito apenas quando algo foi guardado.
        TestOutputHelper.WriteLine($"Navegador: {browserFixture.Settings.Browser}");

        _evidence = new FailureEvidence(
            browserFixture.Settings,
            browserFixture.RunId,
            TestContext.Current.Test!.TestDisplayName);

        _context = await browserFixture.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            BaseURL = browserFixture.Settings.BaseUrl,

            // Sem vídeo ligado, nem grava (nem cria pasta): null desliga a gravação.
            RecordVideoDir = browserFixture.Settings.RecordVideo ? _evidence.EnsureDirectory() : null
        });
        
        _context.SetDefaultTimeout(browserFixture.Settings.Timeout);
        
        // Cria a pasta se o Trace for diferente de OFF
        if (!browserFixture.Settings.RecordVideo && browserFixture.Settings.Trace != TraceMode.Off)
        {
            _evidence.EnsureDirectory();
        }

        // O trace começa logo depois do context, para pegar a criação da página também.
        if (browserFixture.Settings.Trace != TraceMode.Off)
        {
            await _context.Tracing.StartAsync(new TracingStartOptions
            {
                Screenshots = true, // uma imagem a cada passo
                Snapshots = true,   // a cópia navegável da página em cada passo
                Sources = true      // a linha do teste que fez cada passo
            });
        }

        Page = await _context.NewPageAsync();

        await ApplyStressModeAsync();
    }

    // Modo de estresse (ver TestSettings.CpuThrottle). Desligado por padrão.
    private async Task ApplyStressModeAsync()
    {
        var rate = browserFixture.Settings.CpuThrottle;
        if (rate <= 1)
        {
            return;
        }

        // Pedir estresse num navegador que não sabe aplicá-lo e rodar "normal" mesmo assim seria
        // um sucesso que mente: a pessoa acharia que estressou e não estressou.
        if (browserFixture.Settings.Browser != BrowserKind.Chromium)
        {
            throw new InvalidOperationException(
                $"CpuThrottle={rate} só funciona no Chromium, mas o navegador é {browserFixture.Settings.Browser}.");
        }

        var session = await _context.NewCDPSessionAsync(Page);
        await session.SendAsync("Emulation.setCPUThrottlingRate", new Dictionary<string, object> { ["rate"] = rate });
    }

    public async ValueTask DisposeAsync()
    {
        // O xUnit v3 entrega o resultado do teste aqui. (Na v2 isso não existia, e foi o motivo da migração.)
        var testFailed = TestContext.Current.TestState!.Result == TestResult.Failed;

        // O screenshot tem que vir ANTES de fechar o context: depois não há mais tela para fotografar.
        if (testFailed)
        {
            var screenshot = await _evidence.TryTakeScreenshotAsync(Page);
            TestOutputHelper.WriteLine($"Screenshot da falha: {screenshot ?? "não foi possível tirar"}");
        }

        // O trace PARA antes de fechar o context (depois, não há mais o que gravar). Guarda se o teste
        // falhou, ou se pediram para guardar sempre.
        if (browserFixture.Settings.Trace != TraceMode.Off)
        {
            var keepTrace = testFailed || browserFixture.Settings.Trace == TraceMode.Always;
            var trace = await _evidence.StopTraceAsync(_context, keepTrace);
            if (trace is not null)
            {
                TestOutputHelper.WriteLine($"Trace: {trace}");
            }
        }

        // Fechar o context descarta o estado inteiro do teste (nada vaza para o próximo)
        // e termina de gravar o vídeo.
        await _context.CloseAsync();

        if (browserFixture.Settings.RecordVideo)
        {
            var video = await _evidence.FinishVideoAsync(Page, testFailed);
            if (video is not null)
            {
                TestOutputHelper.WriteLine($"Vídeo da falha: {video}");
            }
        }
    }
}