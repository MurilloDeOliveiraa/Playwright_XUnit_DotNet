namespace PlaywrightFramework.Tests;

/// <summary>
/// Os nomes das etiquetas dos testes, num lugar só. O workflow filtra por "Category=Smoke":
/// se mudar aqui, mude lá (.github/workflows/testes.yml).
///
/// Dois jeitos de marcar um teste, equivalentes para o xUnit:
///   [Smoke]                              — o atributo próprio (SmokeAttribute)
///   [Trait(Tags.Category, Tags.Smoke)]   — o nativo, sem texto solto
/// </summary>
public static class Tags
{
    public const string Category = "Category";
    public const string Smoke = "Smoke";
}
