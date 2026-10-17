using System.Globalization;
using Microsoft.Playwright;
using PlaywrightFramework.Configuration;

namespace PlaywrightFramework.Evidence;

/// <summary>
/// Evidência de um teste que falhou: um screenshot (o momento da falha) e um vídeo (o caminho até ela).
/// Só fica no disco quando o teste FALHA — guardar de todos esconderia o que importa.
///
/// Caminho: test-results / execução / navegador / nome do teste.
///   - a execução (data e hora) separa uma rodada da outra: um teste que passa NUNCA apaga a
///     evidência de uma falha anterior, e as falhas da mesma rodada ficam juntas;
///   - o navegador no caminho responde "falhou em qual navegador?" sem precisar abrir nada.
///
/// O screenshot é tirado NA HORA da falha. O vídeo não pode ser: não dá para filmar o que já passou.
/// Por isso ele é gravado em todos os testes e apagado no fim dos que passaram.
///
/// As execuções com falha são apagadas depois de EvidenceRetentionDays (ver DeleteOldRuns).
/// </summary>
public class FailureEvidence
{
    /// <summary>Formato do nome da pasta de cada execução. Serve para criar o nome e para ler a idade dele.</summary>
    public const string RunIdFormat = "yyyyMMdd-HHmmss";

    private readonly string _root;
    private readonly string _directory;

    public FailureEvidence(TestSettings settings, string runId, string testName)
    {
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

    /// <summary>Onde o Playwright grava o vídeo. Nunca apaga nada: pasta que já existe é evidência de outro teste.</summary>
    public string PrepareVideoDirectory()
    {
        Directory.CreateDirectory(_directory);
        return _directory;
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
            var path = Path.Combine(_directory, "failure.png");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
            return path;
        }
        catch (PlaywrightException)
        {
            return null;
        }
    }

    /// <summary>Chamar DEPOIS de fechar o context: é nesse momento que o vídeo termina de ser gravado.</summary>
    public async Task<string?> FinishVideoAsync(IPage page, bool testFailed)
    {
        var video = page.Video ?? throw new InvalidOperationException("O vídeo não foi ligado no context.");

        if (!testFailed)
        {
            await video.DeleteAsync();
            Directory.Delete(_directory, recursive: true);
            DeleteEmptyParents();
            return null;
        }

        return await video.PathAsync();
    }

    // Sobe de pasta em pasta (navegador, depois execução) removendo as que ficaram vazias, e para
    // na raiz. Uma rodada 100% verde não deixa casca nenhuma.
    // Seguro hoje porque os testes rodam um de cada vez. Com paralelismo (fase Escala), conferir
    // "está vazia?" e apagar vira uma corrida: o Delete precisará tolerar IOException.
    private void DeleteEmptyParents()
    {
        var directory = Path.GetDirectoryName(_directory)!;

        while (directory != _root && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
            directory = Path.GetDirectoryName(directory)!;
        }
    }

    private static string SafeFileName(string name) =>
        string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
}
