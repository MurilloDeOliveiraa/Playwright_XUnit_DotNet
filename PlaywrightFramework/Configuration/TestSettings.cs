using Microsoft.Extensions.Configuration;

namespace PlaywrightFramework.Configuration;

/// <summary>
/// As decisões que mudam conforme QUEM roda e ONDE — separadas das que nunca mudam.
///
/// É um tipo, e não um monte de chamadas a config["chave"] espalhadas pelo código, pelo mesmo
/// motivo de User e DeliveryInfo: erro de digitação vira erro de compilação, e não um valor
/// vazio que só aparece no meio do teste.
/// </summary>
public record TestSettings
{
    /// <summary>Endereço do site sob teste.</summary>
    public string BaseUrl { get; init; } = "https://www.saucedemo.com";

    /// <summary>false abre o navegador na tela — útil para acompanhar uma investigação.</summary>
    public bool Headless { get; init; } = true;

    /// <summary>Atraso proposital entre as ações, em milissegundos, para dar tempo de ver.</summary>
    public int SlowMoMilliseconds { get; init; }

    /// <summary>
    /// Monta a configuração empilhando as fontes.
    ///
    /// A ORDEM É O PONTO: cada fonte adicionada depois sobrescreve as anteriores.
    /// A regra por trás — quanto mais específica e mais passageira a fonte, mais alto ela ganha:
    ///
    ///   appsettings.json   → vale para o projeto inteiro, está no repositório  (mais geral)
    ///   User Secrets       → vale para a SUA máquina, fora do repositório
    ///   variável de ambiente → vale para aquela máquina/execução               (mais específica)
    ///
    /// Sobre a linha de comando: a regra geral coloca ela acima de tudo, e é assim que um
    /// aplicativo normal funciona. Aqui ela NÃO foi ligada de propósito — o 'dotnet test'
    /// levanta um processo separado para rodar os testes, e os argumentos digitados ficam no
    /// processo de fora, sem chegar até aqui. Ligar seria código morto. Na prática, para uma
    /// suíte de testes, a variável de ambiente é a camada de cima.
    /// </summary>
    public static TestSettings Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)

            // O "cofre" da máquina local. optional: true porque quem acabou de clonar o
            // repositório ainda não tem nenhum segredo guardado — e os testes precisam rodar
            // assim mesmo. Hoje este projeto não lê nenhum segredo daqui (a senha do SauceDemo
            // é pública); a fonte está ligada para que o lugar certo já exista no dia em que
            // aparecer um segredo de verdade. Ver 'dotnet user-secrets' na nota 11.
            .AddUserSecrets<TestSettings>(optional: true)

            // Prefixo evita colidir com outras variáveis da máquina. O prefixo é removido:
            // SAUCEDEMO_Headless=false  →  a chave lida é "Headless".
            .AddEnvironmentVariables(prefix: "SAUCEDEMO_")

            .Build();

        // Get<T> preenche o tipo a partir das chaves. Se o arquivo não tiver alguma delas,
        // valem os valores padrão declarados acima nas propriedades.
        return configuration.Get<TestSettings>() ?? new TestSettings();
    }
}
