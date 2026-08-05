namespace PlaywrightFramework.Fixtures;

/// <summary>
/// Liga o BrowserFixture a uma coleção de testes.
/// Toda classe marcada com [Collection(Name)] compartilha a MESMA instância do fixture
/// (= o mesmo browser). Custo aceito por enquanto: testes da coleção não rodam em paralelo
/// entre si — assunto da fase "Escala".
/// </summary>
[CollectionDefinition(Name)]
public class PlaywrightCollection : ICollectionFixture<BrowserFixture>
{
    public const string Name = "Playwright collection";

    // Classe vazia de propósito: serve só de "âncora" para os atributos.
}
