namespace PlaywrightFramework.Configuration;

/// <summary>
/// Os navegadores que a suíte sabe usar. Lista FECHADA de propósito.
///
/// Se o nome viesse como texto livre, "firefx" seria um valor possível — e a suíte teria que
/// decidir o que fazer com ele. Cair no Chromium em silêncio seria o pior caminho: o pipeline
/// ficaria verde e ninguém teria testado o Firefox. Com a lista fechada, "firefx" simplesmente
/// não existe, e o erro aparece na hora.
/// </summary>
public enum BrowserKind
{
    Chromium,
    Firefox,
    Webkit
}
