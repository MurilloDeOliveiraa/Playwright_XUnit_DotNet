using Xunit.v3;

namespace PlaywrightFramework.Tests;

/// <summary>
/// Marca um teste como smoke: poucos testes críticos, que rodam primeiro. Quem NÃO tem a marca
/// entra na regressão sozinho, então um teste novo nunca fica de fora por esquecimento.
///
/// Só vale para MÉTODO, de propósito: um [Smoke] na classe transformaria todos os testes dela em smoke.
/// Os nomes da etiqueta moram em <see cref="Tags"/>; o workflow filtra por "Category=Smoke" (se mudar lá, mude no yml).
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class SmokeAttribute : Attribute, ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
        [new(Tags.Category, Tags.Smoke)];
}

// Você não chama o GetTraits(). Quem chama é o xUnit, sozinho, na hora em que ele descobre quais testes existem.
//
//     O caminho é este:
//
// Você roda dotnet test.
//     Antes de executar qualquer coisa, o xUnit lista os testes da sua DLL. Para cada método com [Fact], ele olha todas as etiquetas (atributos) que estão em cima dele.
//     Para cada etiqueta, ele pergunta: “você é do tipo ITraitAttribute?”. Se for, ele chama o GetTraits() e guarda o resultado junto com o teste. No nosso caso, o par Category = Smoke.
//     Só depois, o filtro (--filter-trait "Category=Smoke") compara com esses pares guardados e escolhe quais testes rodam.