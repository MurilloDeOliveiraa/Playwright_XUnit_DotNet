using System.Globalization;
using Microsoft.Playwright;
using PlaywrightFramework.Configuration;

namespace PlaywrightFramework.Evidence;

/// <summary>
/// Evidência de um teste: screenshot (o momento da falha), trace (passo a passo, opcional) e vídeo
/// (opcional). O screenshot só fica no disco quando o teste FALHA — guardar de todos esconderia o
/// que importa. Trace e vídeo são ligados por configuração (TestSettings.Trace e RecordVideo).
///
/// Caminho: test-results / execução / navegador / nome do teste.
///   - a execução (data e hora) separa uma rodada da outra: um teste que passa NUNCA apaga a
///     evidência de uma falha anterior, e as falhas da mesma rodada ficam juntas;
///   - o navegador no caminho responde "falhou em qual navegador?" sem precisar abrir nada;
///   - a pasta do teste só nasce quando há o que guardar (screenshot ou trace salvos). A exceção é o
///     vídeo: com RecordVideo ligado ela nasce antes do teste, porque o Playwright precisa do caminho
///     desde o começo; num teste que passa o vídeo é apagado e a pasta fica vazia até a limpeza do fim.
///
/// O screenshot é tirado NA HORA da falha. Trace e vídeo não podem ser: não dá para gravar o que já
/// passou. Por isso são gravados durante o teste e descartados no fim, quando não for para guardar.
///
/// As execuções com falha são apagadas depois de EvidenceRetentionDays (ver DeleteOldRuns).
/// </summary>
public class FailureEvidence
{
    /// <summary>Formato do nome da pasta de cada execução. Serve para criar o nome e para ler a idade dele.</summary>
    public const string RunIdFormat = "yyyyMMdd-HHmmss";

    private readonly TestSettings _settings;
    private readonly string _root;
    private readonly string _directory;

    public FailureEvidence(TestSettings settings, string runId, string testName)
    {
        _settings = settings;

        // Caminho relativo vale a partir da pasta onde os testes rodam (AppContext.BaseDirectory).
        _root = Path.Combine(AppContext.BaseDirectory, settings.EvidenceDirectory);
        _directory = Path.Combine(_root, runId, settings.Browser.ToString(), SafeFileName(testName));
    }

    /// <summary>
    /// Apaga as execuções mais velhas que o prazo configurado. Chamado uma vez, quando a suíte sobe.
    ///   - a idade vem do NOME da pasta (a hora da execução), não da data do arquivo, que muda se
    ///     alguém copiar ou mover a pasta;
    ///   - pasta que não tem o nosso formato de nome é ignorada: nunca se apaga o que não se reconhece;
    ///   - se não der para apagar (ex.: vídeo aberto no player), segue em frente. É manutenção, não é o
    ///     veredito de um teste, e a próxima execução tenta de novo.
    /// </summary>
    public static void DeleteOldRuns(TestSettings settings)
    {
        if (settings.EvidenceRetentionDays <= 0)
        {
            return;
        }

        var root = Path.Combine(AppContext.BaseDirectory, settings.EvidenceDirectory);
        if (!Directory.Exists(root))
        {
            return;
        }

        var limit = DateTime.Now.AddDays(-settings.EvidenceRetentionDays);

        foreach (var run in Directory.EnumerateDirectories(root))
        {
            var isOurFolder = DateTime.TryParseExact(Path.GetFileName(run), RunIdFormat,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var startedAt);

            if (!isOurFolder || startedAt >= limit)
            {
                continue;
            }

            try
            {
                Directory.Delete(run, recursive: true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    /// <summary>
    /// Cria a pasta do teste, se ainda não existir, e devolve o caminho. É chamada por quem vai gravar
    /// (screenshot e trace salvos) e, com vídeo ligado, pelo BaseTest antes do teste. Nunca apaga nada:
    /// pasta que já existe é evidência de outro teste.
    /// </summary>
    public string EnsureDirectory()
    {
        if (!Directory.Exists(_directory))
            Directory.CreateDirectory(_directory);
        return _directory;
    }

    /// <summary>
    /// A pasta onde o Playwright grava o vídeo, ou null se o vídeo está desligado (null desliga a gravação).
    /// Vai nas opções de criação do context, ou seja, ANTES de o context existir, por isso é separada do trace.
    /// </summary>
    public string? VideoDirectory() => _settings.RecordVideo ? EnsureDirectory() : null;

    /// <summary>
    /// Liga o trace, se a configuração pede. Chamar logo depois de criar o context, para pegar a criação
    /// da página também. Não faz nada com Trace desligado.
    /// </summary>
    public async Task StartTraceAsync(IBrowserContext context)
    {
        if (_settings.Trace == TraceMode.Off)
        {
            return;
        }

        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true, // uma imagem a cada passo
            Snapshots = true,   // a cópia navegável da página em cada passo
            Sources = true      // a linha do teste que fez cada passo
        });
    }

    /// <summary>
    /// Tira o screenshot da tela como ela está agora. Devolve o caminho, ou null se não deu.
    /// Não deixa a exceção subir: uma página já fechada ou travada geraria um SEGUNDO erro que
    /// esconderia o verdadeiro — o mesmo cuidado do DisposeAsync do BrowserFixture.
    /// </summary>
    public async Task<string?> TryTakeScreenshotAsync(IPage page)
    {
        try
        {
            var path = Path.Combine(EnsureDirectory(), "failure.png");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
            return path;
        }
        catch (PlaywrightException)
        {
            return null;
        }
    }

    /// <summary>
    /// Para o trace. Chamar ANTES de fechar o context: depois dele não há mais o que gravar.
    /// Guarda se o teste falhou, ou se pediram para guardar sempre (Trace = Always); senão descarta.
    /// Parar SEM caminho descarta a gravação; com caminho, salva o .zip. Devolve o caminho, ou null se
    /// descartou (ou se o Trace está desligado).
    ///
    /// Diferente do screenshot, o erro daqui SOBE de propósito: um trace que não deu para guardar ou
    /// descartar é um defeito da própria evidência, e deve aparecer. Quem chama garante o fechamento
    /// do context com try/finally.
    /// </summary>
    public async Task<string?> StopTraceAsync(IBrowserContext context, bool testFailed)
    {
        if (_settings.Trace == TraceMode.Off)
        {
            return null;
        }

        var keep = testFailed || _settings.Trace == TraceMode.Always;
        if (!keep)
        {
            await context.Tracing.StopAsync();
            return null;
        }

        var path = Path.Combine(EnsureDirectory(), "trace.zip");
        await context.Tracing.StopAsync(new TracingStopOptions { Path = path });
        return path;
    }

    /// <summary>
    /// Chamar DEPOIS de fechar o context: é nesse momento que o vídeo termina de ser gravado. Apaga o
    /// arquivo se o teste passou; devolve o caminho se ficou (ou null, também com o vídeo desligado).
    /// </summary>
    public async Task<string?> FinishVideoAsync(IPage page, bool testFailed)
    {
        if (!_settings.RecordVideo)
        {
            return null;
        }

        var video = page.Video ?? throw new InvalidOperationException("O vídeo não foi ligado no context.");

        if (!testFailed)
        {
            await video.DeleteAsync();
            return null;
        }

        return await video.PathAsync();
    }

    /// <summary>
    /// Chamar UMA vez, quando a suíte termina (BrowserFixture.DisposeAsync). Remove as pastas VAZIAS
    /// da execução (do teste, do navegador e a da própria execução). Uma rodada sem nada para guardar
    /// não deixa casca nenhuma, e uma pasta com trace ou screenshot dentro nunca é tocada.
    ///
    /// Por que no fim, e não ao fim de cada teste: com testes em paralelo, "a pasta está vazia?"
    /// seguido de "apague" é uma corrida: outro teste pode criar uma subpasta, ou ter acabado de criar
    /// a dele e ainda não ter escrito nada. No fim da suíte nenhum teste está rodando.
    /// </summary>
    public static void DeleteEmptyFolders(TestSettings settings, string runId)
    {
        var runDirectory = Path.Combine(AppContext.BaseDirectory, settings.EvidenceDirectory, runId);
        if (!Directory.Exists(runDirectory))
        {
            return;
        }

        // Manutenção, não veredito: uma pasta vazia que sobra é só casca, e a retenção a remove depois.
        // Por isso nenhum erro daqui pode subir (iria quebrar o DisposeAsync do fixture e esconder o
        // erro verdadeiro), e cada pasta tem o próprio try: uma pasta travada não impede as outras.
        List<string> folders;
        try
        {
            // De baixo para cima: o mais fundo primeiro, para a pasta de cima já ficar vazia quando chegar a vez dela.
            folders = Directory.EnumerateDirectories(runDirectory, "*", SearchOption.AllDirectories)
                .Append(runDirectory)
                .OrderByDescending(folder => folder.Length)
                .ToList();
        }
        catch (IOException)
        {
            return;
        }
        catch (UnauthorizedAccessException)
        {
            return;
        }

        foreach (var folder in folders)
        {
            try
            {
                if (!Directory.EnumerateFileSystemEntries(folder).Any())
                {
                    Directory.Delete(folder);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static string SafeFileName(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
