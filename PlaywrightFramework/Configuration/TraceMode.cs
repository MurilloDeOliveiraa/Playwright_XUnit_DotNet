namespace PlaywrightFramework.Configuration;

/// <summary>
/// Quando gravar o trace (a gravação passo a passo que abre no Trace Viewer).
/// Lista fechada, como o BrowserKind: um valor com erro de digitação para a execução, em vez de
/// virar "desligado" em silêncio — um erro que aparece é melhor que um sucesso que mente.
/// </summary>
public enum TraceMode
{
    /// <summary>Não grava. Gravar custa tempo em todos os testes; use quando a velocidade importar mais que a evidência.</summary>
    Off,

    /// <summary>Grava em todos e guarda só o dos testes que falharam. Bom para caçar falha instável.</summary>
    OnFailure,

    /// <summary>Grava e guarda, mesmo se o teste passou. Bom para investigar um teste lento que passa.</summary>
    Always
}
